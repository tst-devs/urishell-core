using UriShell.Shell.Registration;

namespace UriShell.Shell.Resolution
{
    public interface IResolveSetupFactory
    {
        IShellResolveSetup<TResolved> Create<TResolved>(ResolveSetupArgs args);
    }
}