// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCPStats
{
    public class Config : IConfig
    {
        [Description("Is the plugin enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should debug messages be shown in a server console.")]
        public bool Debug { get; set; } = false;

        [Description("Should developers get credit if they join the server?")]
        public bool ShowDeveloperBadge { get; set; } = true;

        [Description("Enable automatic plugin updates.")]
        public bool EnableAutoUpdate { get; set; } = true;


    }
}
