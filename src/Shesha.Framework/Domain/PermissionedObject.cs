﻿using System;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Shesha.Domain.Attributes;
using Shesha.Domain.Enums;

namespace Shesha.Domain
{
    [Entity(TypeShortAlias = "Shesha.Framework.PermissionedObject", GenerateApplicationService = false)]
    public class PermissionedObject : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// Text identifier of the object (for example, the full name of the class)
        /// </summary>
        [NotNull]
        public virtual string Object { get; set; }

        /// <summary>
        /// Category for grouping objects
        /// </summary>
        public virtual string Category { get; set; }

        /// <summary>
        /// Shesha Module of the permissioned object
        /// </summary>
        [NotNull]
        public virtual string Module { get; set; }

        /// <summary>
        /// Type of the permissioned object
        /// </summary>
        [NotNull]
        public virtual string Type { get; set; }

        /// <summary>
        /// Name for display in the configurator
        /// </summary>
        [NotNull]
        public virtual string Name { get; set; }
        
        /// <summary>
        /// Description for display in the configurator
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// List of permissions required to access this securable (comma-separated permission identifiers)
        /// </summary>
        public virtual string Permissions { get; set; }

        /// <summary>
        /// Object inherits permissions from parent object
        /// </summary>
        public virtual bool Inherited => Access == RefListPermissionedAccess.Inherited;

        /// <summary>
        /// Access type
        /// </summary>
        public virtual RefListPermissionedAccess Access { get; set; }

        /// <summary>
        /// Text identifier of the parent object
        /// </summary>
        public virtual string Parent { get; set; }

        /// <summary>
        /// Dependence on another permissioned object (for example, CRUD API on an entity)
        /// </summary>
        public virtual string Dependency { get; set; }

        public virtual bool Hidden { get; set; }
    }
}