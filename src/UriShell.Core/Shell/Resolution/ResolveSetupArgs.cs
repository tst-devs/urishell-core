using System;
using UriShell.Shell.Registration;

namespace UriShell.Shell.Resolution
{
    public sealed class ResolveSetupArgs
    {
        public ResolveSetupArgs(IShellResolveOpen resolveOpen, Action<ResolveSetupPlayer> playerSender)
        {
            ResolveOpen = resolveOpen ?? throw new ArgumentNullException(nameof(resolveOpen));
            PlayerSender = playerSender ?? throw new ArgumentNullException(nameof(playerSender));
        }

        public IShellResolveOpen ResolveOpen { get; }

        public Action<ResolveSetupPlayer> PlayerSender { get; }
    }
}
