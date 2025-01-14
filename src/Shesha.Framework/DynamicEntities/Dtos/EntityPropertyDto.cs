﻿using Abp.Application.Services.Dto;
using Shesha.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shesha.DynamicEntities.Dtos
{
    /// <summary>
    /// Entity property DTO
    /// </summary>
    public class EntityPropertyDto : EntityDto<Guid>
    {
        /// <summary>
        /// Property Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Label (display name)
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Data format
        /// </summary>
        public string DataFormat { get; set; }

        /// <summary>
        /// Entity type. Aplicable for entity references
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Reference list name
        /// </summary>
        public string ReferenceListName { get; set; }

        /// <summary>
        /// Reference list namespace
        /// </summary>
        public string ReferenceListNamespace { get; set; }

        /// <summary>
        /// Source type (ApplicationCode = 1, UserDefined = 2)
        /// </summary>
        public MetadataSourceType? Source { get; set; }

        /// <summary>
        /// Child properties, applicable for complex data types (e.g. object, array)
        /// </summary>
        public List<EntityPropertyDto> Properties { get; set; } = new List<EntityPropertyDto>();

        /// <summary>
        /// Items type (applicable for arrays)
        /// </summary>
        public EntityPropertyDto ItemsType { get; set; }
    }
}
