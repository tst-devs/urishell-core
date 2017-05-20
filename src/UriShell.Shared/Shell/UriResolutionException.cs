using System;

namespace UriShell.Shell
{
    public class UriResolutionException : Exception
    {
        public UriResolutionException(Uri uri, string message)
            : base(message)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public UriResolutionException(Uri uri, string message, Exception innerException)
            : base(message, innerException)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public Uri Uri { get; }

        public override string Message => $"{base.Message}{Environment.NewLine}URI: \"{Uri}\"";
    }
}
