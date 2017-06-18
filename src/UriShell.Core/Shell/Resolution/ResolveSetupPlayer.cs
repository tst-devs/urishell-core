using System;
using Microsoft.Extensions.Logging;

namespace UriShell.Shell.Resolution
{
    public delegate void ResolveSetupPlayer(Uri uri, object resolved, ILogger logger, ShellDisposable disposable);
}
