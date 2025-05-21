// -----------------------------------------------------------------------
// <copyright file="Killlogger.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;

namespace SCPStats
{
    public static class KillLogger
    {
        private static readonly List<KillLog> kills = new();

        public static void LogKill(DiedEventArgs ev)
        {
            var player = ev.Player;
            var killer = ev.Attacker;

            var log = new KillLog
            {
                PlayerName = player.Nickname,
                PlayerId = player.UserId,
                PlayerClass = player.Role.ToString(),

                KillerName = killer?.Nickname ?? "World",
                KillerId = killer?.UserId ?? "N/A",
                KillerClass = killer?.Role.ToString() ?? "None",

                DamageType = ev.DamageHandler.Type.ToString(),
                WeaponOrCause = ev.DamageHandler.ToString(),

                Time = DateTime.UtcNow.ToString("o"),
                RoundTimeSeconds = Round.ElapsedTime.TotalSeconds
            };

            lock (kills)
            {
                kills.Add(log);
            }
        }

        public static List<KillLog> RetrieveAndClear()
        {
            lock (kills)
            {
                var list = new List<KillLog>(kills);
                kills.Clear();
                return list;
            }
        }
    }

    public class KillLog
    {
        public string PlayerName;
        public string PlayerId;
        public string PlayerClass;
        public string PlayerPosition;

        public string KillerName;
        public string KillerId;
        public string KillerClass;

        public string DamageType;
        public string WeaponOrCause;

        public string Time;
        public double RoundTimeSeconds;
    }
}
