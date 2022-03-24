﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Reflection;
using Shesha.Permissions;
using Shesha.Reflection;

namespace Shesha.Permission
{
    public class EntityProtectedObjectProvider : ProtectedObjectProviderBase, IProtectedObjectProvider
    {

        public EntityProtectedObjectProvider(IAssemblyFinder assembleFinder) : base(assembleFinder)
        {
        }

        public const string ObjectCategory = "entity";

        public string GetCategory()
        {
            return ObjectCategory;
        }

        public string GetCategoryByType(Type type)
        {
            var entityType = typeof(IEntity<>);

            return type.IsPublic && !type.IsAbstract 
                   && type.GetInterfaces().Any(x =>
                       x.IsGenericType &&
                       x.GetGenericTypeDefinition() == entityType)
                ? ObjectCategory
                : null;
        }

        public List<ProtectedObjectDto> GetAll()
        {
            // ToDo: add Entities to configured permissions
            return new List<ProtectedObjectDto>();

            var assemblies = _assembleFinder.GetAllAssemblies().Distinct(new AssemblyFullNameComparer()).Where(a => !a.IsDynamic).ToList();
            var allPermissions = new List<ProtectedObjectDto>();

            var entityType = typeof(IEntity<>);

            foreach (var assembly in assemblies)
            {
                var services = assembly.GetTypes()
                    .Where(t => t.IsPublic && !t.IsAbstract && t.GetInterfaces().Any(x =>
                                    x.IsGenericType &&
                                    x.GetGenericTypeDefinition() == entityType))
                    .ToList();
                foreach (var service in services)
                {
                    var parent = new ProtectedObjectDto()
                    {
                        Object = service.FullName, 
                        Category = ObjectCategory, 
                        Description = GetDescription(service)
                    };
                    allPermissions.Add(parent);


                    var child = new ProtectedObjectDto()
                    {
                        Object = service.FullName + "@Create", 
                        Category = ObjectCategory, 
                        Parent = service.FullName, 
                        Description = "Create"
                    };
                    allPermissions.Add(child);

                    child = new ProtectedObjectDto()
                    {
                        Object = service.FullName + "@Update",
                        Category = ObjectCategory,
                        Parent = service.FullName,
                        Description = "Update"
                    };
                    allPermissions.Add(child);

                    child = new ProtectedObjectDto()
                    {
                        Object = service.FullName + "@Delete",
                        Category = ObjectCategory,
                        Parent = service.FullName,
                        Description = "Delete"
                    };
                    allPermissions.Add(child);

                    child = new ProtectedObjectDto()
                    {
                        Object = service.FullName + "@Get",
                        Category = ObjectCategory,
                        Parent = service.FullName,
                        Description = "Get"
                    };
                    allPermissions.Add(child);
                }
            }

            return allPermissions;
        }
    }
}