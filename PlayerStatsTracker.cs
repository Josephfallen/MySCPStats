using System.Collections.Generic;

namespace SCPStats
{
    public static class PlayerStatsTracker
    {
        private static readonly Dictionary<string, int> GamesPlayed = new();

        public static void RegisterPlayer(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            if (GamesPlayed.ContainsKey(userId))
                GamesPlayed[userId]++;
            else
                GamesPlayed[userId] = 1;
        }

        public static Dictionary<string, int> GetAll() => new(GamesPlayed);

        public static void Reset()
        {
            GamesPlayed.Clear();
        }
    }
}
