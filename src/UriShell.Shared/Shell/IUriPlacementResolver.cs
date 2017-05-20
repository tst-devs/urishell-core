using System;

namespace UriShell.Shell
{
	public interface IUriPlacementResolver
	{
		IUriPlacementConnector Resolve(object resolved, Uri uri, UriAttachmentSelector attachmentSelector);
	}
}
