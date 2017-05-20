using System;

namespace UriShell.Shell.Registration
{
	public interface IShellResolveOpen
	{
		IDisposable Open();

		IDisposable OpenOrThrow();
	}
}
