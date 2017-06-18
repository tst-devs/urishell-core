using System;

namespace UriShell.Shell
{
	public sealed class UriModuleEntitledItemResolver<T> : IUriModuleItemResolver
	{
		private readonly Func<string, T> _entitledItemFactory;

		public UriModuleEntitledItemResolver(Func<string, T> entitledItemFactory)
		{
			_entitledItemFactory = entitledItemFactory;
		}

		public object Resolve(Uri uri, UriAttachmentSelector attachmentSelector)
        {
			var uriBuilder = new ShellUriBuilder(uri);
			var title = uriBuilder.Parameters["title"] ?? uri.ToString();

			return _entitledItemFactory(title);	
		}
	}
}
