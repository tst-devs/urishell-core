using System;

namespace UriShell.Shell
{
    public sealed class ResolvedIdBroadcastArgs
    {
        public ResolvedIdBroadcastArgs(int resolvedId)
        {
            ResolvedId = resolvedId;
        }

        public int ResolvedId { get; private set; }
    }
}
