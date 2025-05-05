using System.Collections.Generic;
using System.Threading.Tasks;
using MEC;

namespace SCPStats
{
    public static class TimerHandler
    {
        private static CoroutineHandle _killUploadCoroutine;
        private static bool _isRoundInProgress;

        public static void Initialize()
        {
            // Starts the coroutine loop for uploading kills
            _killUploadCoroutine = Timing.RunCoroutine(KillUploadLoop());
        }

        public static void RoundStarted()
        {
            _isRoundInProgress = true;
        }

        public static void Dispose()
        {
            // Stops the coroutine when no longer needed
            Timing.KillCoroutines(_killUploadCoroutine);
        }

        private static IEnumerator<float> KillUploadLoop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(10f); // Wait 10 seconds

                // Run the async method safely on a background thread
                Task.Run(async () => await WebSender.UploadKillsWithTokenAsync());
            }
        }
    }
}
