﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shesha.FluentMigrator.Notifications
{
    public interface IAddNotificationTemplateSyntax
    {
        /// <summary>
        /// Disable template
        /// </summary>
        IAddNotificationTemplateSyntax Disable();

        /// <summary>
        /// Enable template
        /// </summary>
        IAddNotificationTemplateSyntax Enable();
    }
}
