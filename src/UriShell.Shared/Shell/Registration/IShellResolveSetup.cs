using System;

namespace UriShell.Shell.Registration
{
    public interface IShellResolveSetup<TResolved> : IShellResolveOpen
    {
        IShellResolveSetup<TResolved> OnReady(Action<TResolved> action);

        IShellResolveSetup<TResolved> OnFinished(Action<TResolved> action);
    }
}
