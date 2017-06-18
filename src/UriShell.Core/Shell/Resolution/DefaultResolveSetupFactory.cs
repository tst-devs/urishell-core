using UriShell.Shell.Registration;

namespace UriShell.Shell.Resolution
{
	public sealed class DefaultResolveSetupFactory : IResolveSetupFactory
	{
		public IShellResolveSetup<TResolved> Create<TResolved>(ResolveSetupArgs args)
		{
			return new ResolveSetup<TResolved>(args);
		}
	}
}
