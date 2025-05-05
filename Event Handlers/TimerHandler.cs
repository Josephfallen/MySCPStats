using System.Collections.Generic;
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
                yield return Timing.WaitForSeconds(900f); // Wait 15 minutes

                // Call your async kill upload method here
                _ = WebSender.UploadKillsWithTokenAsync(); // Fire-and-forget
            }
        }
    }
}
