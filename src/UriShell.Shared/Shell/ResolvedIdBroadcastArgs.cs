using System;

namespace UriShell.Shell
{
	public sealed class ResolvedIdBroadcastArgs
	{
		public ResolvedIdBroadcastArgs(int resolvedId)
		{
            ShellUriBuilder.CheckOwnerId(resolvedId);
			ResolvedId = resolvedId;
		}

		public int ResolvedId { get; private set; }
	}
}
