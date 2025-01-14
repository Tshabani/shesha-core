﻿using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Runtime.Validation;
using GraphQL;
using Microsoft.AspNetCore.Mvc;
using Shesha.Application.Services.Dto;
using Shesha.DynamicEntities;
using Shesha.DynamicEntities.Cache;
using Shesha.DynamicEntities.Dtos;
using Shesha.GraphQL.Middleware;
using Shesha.GraphQL.Mvc;
using Shesha.GraphQL.Provider;
using Shesha.Metadata;
using Shesha.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shesha
{
    [DynamicControllerNameConvention]
    public class DynamicCrudAppService<TEntity, TDynamicDto, TPrimaryKey> : SheshaCrudServiceBase<TEntity,
        TDynamicDto, TPrimaryKey, FilteredPagedAndSortedResultRequestDto, TDynamicDto, TDynamicDto, GetDynamicEntityInput<TPrimaryKey>>, IDynamicCrudAppService<TEntity, TDynamicDto, TPrimaryKey>, ITransientDependency
        where TEntity : class, IEntity<TPrimaryKey>
        where TDynamicDto : class, IDynamicDto<TEntity, TPrimaryKey>
    {
        public DynamicCrudAppService(IRepository<TEntity, TPrimaryKey> repository) : base(repository)
        {
        }

        public override async Task<TDynamicDto> GetAsync(GetDynamicEntityInput<TPrimaryKey> input)
        {
            CheckGetPermission();

            var entity = await Repository.GetAsync(input.Id);

            return await MapToCustomDynamicDtoAsync<TDynamicDto, TEntity, TPrimaryKey>(entity);
        }


        public override async Task<TDynamicDto> UpdateAsync(TDynamicDto input)
        {
            CheckUpdatePermission();

            var entity = await Repository.GetAsync(input.Id);

            await MapDynamicDtoToEntityAsync<TDynamicDto, TEntity, TPrimaryKey>(input, entity);

            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationResults))
                throw new AbpValidationException("Please correct the errors and try again", validationResults);

            await Repository.UpdateAsync(entity);

            return await MapToCustomDynamicDtoAsync<TDynamicDto, TEntity, TPrimaryKey>(entity);
        }

        public override async Task<TDynamicDto> CreateAsync(TDynamicDto input)
        {
            CheckCreatePermission();

            var entity = Activator.CreateInstance<TEntity>();

            await MapStaticPropertiesToEntityDtoAsync<TDynamicDto, TEntity, TPrimaryKey>(input, entity);

            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationResults))
                throw new AbpValidationException("Please correct the errors and try again", validationResults);

            await Repository.InsertAsync(entity);

            await UnitOfWorkManager.Current.SaveChangesAsync();

            await MapDynamicPropertiesToEntityAsync<TDynamicDto, TEntity, TPrimaryKey>(input, entity);

            await UnitOfWorkManager.Current.SaveChangesAsync();

            return await MapToCustomDynamicDtoAsync<TDynamicDto, TEntity, TPrimaryKey>(entity);
        }

        public override async Task<PagedResultDto<TDynamicDto>> GetAllAsync(FilteredPagedAndSortedResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var totalCount = await AsyncQueryableExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var entities = await AsyncQueryableExecuter.ToListAsync(query);

            var list = new List<TDynamicDto>();
            foreach (var entity in entities)
            {
                list.Add(await MapToCustomDynamicDtoAsync<TDynamicDto, TEntity, TPrimaryKey>(entity));
            }

            return new PagedResultDto<TDynamicDto>(
                totalCount,
                list
            );
        }

        #region GraphQL

        /// <summary>
        /// GraphQL document executer
        /// </summary>
        public IDocumentExecuter DocumentExecuter { get; set; }
        public ISchemaContainer SchemaContainer { get; set; }
        public IGraphQLSerializer Serializer { get; set; }
        public IEntityConfigCache EntityConfigCache { get; set; }

        /// <summary>
        /// Query entity data. 
        /// NOTE: don't use on prod, will be merged with the `Get`endpoint soon
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">NOTE: shape of the `result` depends on the `properties` argument. When `properties` argument is not specified - it returns top level properties of the entity, all referenced entities are presented as their Id values</response>
        [HttpGet]
        public virtual async Task<GraphQLDataResult<TEntity>> QueryAsync(GetDynamicEntityInput<TPrimaryKey> input)
        {
            CheckGetAllPermission();

            var schemaName = Abp.Extensions.StringExtensions.ToCamelCase(typeof(TEntity).Name);

            var schema = await SchemaContainer.GetOrDefaultAsync(schemaName);
            var httpContext = AppContextHelper.Current;

            var result = await DocumentExecuter.ExecuteAsync(async s =>
            {
                s.Schema = schema;

                var properties = string.IsNullOrWhiteSpace(input.Properties)
                    ? await GetGqlTopLevelPropertiesAsync()
                    : input.Properties;

                s.Query = $@"query{{
  {schemaName}(id: ""{input.Id}"") {{
    {properties}
  }}
}}";

                if (httpContext != null)
                {
                    s.RequestServices = httpContext.RequestServices;
                    s.UserContext = new GraphQLUserContext
                    {
                        User = httpContext.User,
                    };
                    s.CancellationToken = httpContext.RequestAborted;
                }
            });

            if (result.Errors != null)
                throw new AbpValidationException("", result.Errors.Select(e => new ValidationResult(e.Message)).ToList());

            return new GraphQLDataResult<TEntity>(result);
        }

        /// <summary>
        /// Query entities list
        /// NOTE: don't use on prod, will be merged with the GetAll endpoint soon
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">NOTE: shape of the `items[]` depends on the `properties` argument. When `properties` argument is not specified - it returns top level properties of the entity, all referenced entities are presented as their Id values</response>
        [HttpGet]
        public virtual async Task<GraphQLDataResult<PagedResultDto<TEntity>>> QueryAllAsync(PropsFilteredPagedAndSortedResultRequestDto input)
        {
            CheckGetAllPermission();

            var schemaName = Abp.Extensions.StringExtensions.ToCamelCase(typeof(TEntity).Name);

            var schema = await SchemaContainer.GetOrDefaultAsync(schemaName);
            var httpContext = AppContextHelper.Current;

            var result = await DocumentExecuter.ExecuteAsync(async s =>
            {
                s.Schema = schema;

                var properties = string.IsNullOrWhiteSpace(input.Properties)
                    ? await GetGqlTopLevelPropertiesAsync()
                    : input.Properties;

                s.Query = $@"query getAll($filter: String, $quickSearch: String, $sorting: String, $skipCount: Int, $maxResultCount: Int){{
  {schemaName}List(input: {{ filter: $filter, quickSearch: $quickSearch, sorting: $sorting, skipCount: $skipCount, maxResultCount: $maxResultCount }}){{
    totalCount
    items {{
        {properties}
    }}
  }}
}}";
                s.Variables = new Inputs(new Dictionary<string, object> {
                    { "filter", input.Filter },
                    { "quickSearch", input.QuickSearch },
                    { "sorting", input.Sorting },
                    { "skipCount", input.SkipCount },
                    { "maxResultCount", input.MaxResultCount },
                });

                if (httpContext != null)
                {
                    s.RequestServices = httpContext.RequestServices;
                    s.UserContext = new GraphQLUserContext
                    {
                        User = httpContext.User,
                    };
                    s.CancellationToken = httpContext.RequestAborted;
                }
            });

            if (result.Errors != null)
                throw new AbpValidationException("", result.Errors.Select(e => new ValidationResult(e.Message)).ToList());

            return new GraphQLDataResult<PagedResultDto<TEntity>>(result);
        }

        private void AppendProperty(StringBuilder sb, EntityPropertyDto property)
        {
            // todo: use FieldNameConverter to get correct case of the field names
            var propertyName = property.Name.ToCamelCase();

            switch (property.DataType)
            {
                case DataTypes.Array:
                    // todo: implement and uncomment
                    return;

                case DataTypes.EntityReference:
                    sb.AppendLine($"{propertyName}: {propertyName}{nameof(IEntity.Id)}");
                    break;

                case DataTypes.Object:
                    {
                        sb.Append(propertyName);
                        sb.AppendLine("{");
                        foreach (var subProp in property.Properties)
                        {
                            AppendProperty(sb, subProp);
                        }
                        sb.AppendLine("}");
                        break;
                    }
                default:
                    sb.AppendLine(propertyName);
                    break;
            }
        }

        private async Task<string> GetGqlTopLevelPropertiesAsync()
        {
            var sb = new StringBuilder();
            var properties = await EntityConfigCache.GetEntityPropertiesAsync(typeof(TEntity));
            foreach (var property in properties) 
            {
                AppendProperty(sb, property);
            }

            return sb.ToString();
        }

        #endregion
    }
}
