﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shesha.Web.DataTable.Model
{
    /// <summary>
    /// Get datatable columns input. Is used for tables with configurable columns
    /// </summary>
    public class GetColumnsInput
    {
        /// <summary>
        /// Type of entity
        /// </summary>
        [Required]
        public string EntityType { get; set; }

        /// <summary>
        /// List of property names
        /// </summary>
        public List<string> Properties { get; set; } = new List<string>();
    }
}
