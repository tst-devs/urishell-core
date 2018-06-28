using System;

namespace UriShell.Shell.Resolution
{
    public struct UriResolvedMetadata
    {
        public UriResolvedMetadata(Uri uri, IDisposable disposable) : this()
        {
            Uri = uri;
            Disposable = disposable;
        }

        private UriResolvedMetadata(UriResolvedMetadata source, int resolvedId)
        {
            this = source;
            ResolvedId = resolvedId;
        }

        public UriResolvedMetadata AssignId(int resolvedId)
        {
            return new UriResolvedMetadata(this, resolvedId);
        }

        public Uri Uri { get; }

        public IDisposable Disposable { get; }

        public int ResolvedId { get; }
    }
}
