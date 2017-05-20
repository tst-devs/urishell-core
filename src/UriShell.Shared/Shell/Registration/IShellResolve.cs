namespace UriShell.Shell.Registration
{
    public interface IShellResolve : IShellResolveOpen
    {
        IShellResolveSetup<TResolved> Setup<TResolved>();
    }
}
