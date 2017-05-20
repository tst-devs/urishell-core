using System;

namespace UriShell.Shell
{
	public interface IUriModuleItemResolver
	{
		object Resolve(Uri uri, UriAttachmentSelector attachmentSelector);
	}
}
