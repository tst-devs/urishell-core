using System;

namespace UriShell.Extensions
{
	public static class UriExtensions
	{
		public static bool IsUriShell(this Uri uri)
		{
            if (uri == null)
            {
                throw new ArgumentException(nameof(uri));
            }
			if (!uri.IsAbsoluteUri)
			{
                throw new InvalidOperationException("Can't validate the scheme; URI isn't absolute.");
			}

			return string.CompareOrdinal(uri.Scheme, UriShellSettings.Scheme) == 0;
		}
	}
}
