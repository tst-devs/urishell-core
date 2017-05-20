using System;

namespace UriShell.Shell
{
    public sealed class UriModuleParameterlessItemResolver<T> : IUriModuleItemResolver
    {
        private readonly Func<T> _factory;

        public UriModuleParameterlessItemResolver(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public object Resolve(Uri uri, UriAttachmentSelector attachmentSelector)
        {
            return _factory();
        }
    }
}
