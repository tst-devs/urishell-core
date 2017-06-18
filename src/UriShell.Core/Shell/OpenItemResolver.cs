using System;
using System.Diagnostics;

namespace UriShell.Shell
{
	internal sealed class OpenItemResolver : IUriModuleItemResolver
	{
		public object Resolve(Uri uri, UriAttachmentSelector attachmentSelector)
		{
            var builder = new ShellUriBuilder(uri);
			var fileName = builder.Parameters["fileName"];

			if (string.IsNullOrWhiteSpace(fileName))
			{
				return null;
			}

			return new Process
			{
				StartInfo = new ProcessStartInfo(fileName)
			};
		}
	}
}
