// -----------------------------------------------------------------------
// <copyright file="LoaderMessages.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace MySCPStats.Loader
{
    using System;

    /// <summary>
    /// Contains different loader messages for MySCPStats.
    /// </summary>
    public static class LoaderMessages
    {
        /// <summary>
        /// Gets the default loader message.
        /// </summary>
        public static string Default => @"
/$$      /$$  /$$$$$$   /$$$$$$ 
| $$$    /$$$ /$$__  $$ /$$__  $$
| $$$$  /$$$$| $$  \__/| $$  \__/
| $$ $$/$$ $$|  $$$$$$ |  $$$$$$      MySCPStats by joseph_fallen
| $$  $$$| $$ \____  $$ \____  $$
| $$\  $ | $$ /$$  \ $$ /$$  \ $$
| $$ \/  | $$|  $$$$$$/|  $$$$$$/
|__/     |__/ \______/  \______/ 


";


        /// <summary>
        /// Gets the loader message according to the current month and flags.
        /// </summary>
        /// <returns>The corresponding loader message.</returns>
        public static string GetMessage()
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Contains("--defaultloadmessage"))
                return Default;


            return Default;
        }
    }
}
