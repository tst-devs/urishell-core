using System;

namespace UriShell.Shell
{
    public sealed class OneSecondElapsedBroadcastArgs
    {
        public OneSecondElapsedBroadcastArgs(TimeSpan totalElapsed)
        {
            TotalElapsed = totalElapsed;
        }

        public TimeSpan TotalElapsed { get; private set; }
    }
}
