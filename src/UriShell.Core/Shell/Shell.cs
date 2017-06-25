using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UriShell.Extensions;
using UriShell.Shell.Registration;
using UriShell.Shell.Resolution;

namespace UriShell.Shell
{
    using ShellResolveFactory = Func<Uri, object[], IShellResolve>;

    public sealed partial class Shell : IShell, IUriResolutionCustomization
    {
        private static readonly Regex _HyperLinkRegex = new Regex("<a\\s+href=\"([^\"]+)\">(.*)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ShellResolveFactory _shellResolveFactory;

        private readonly IUriResolvedObjectHolder _uriResolvedObjectHolder;

        private readonly WeakRefs<IUriPlacementResolver> _uriPlacementResolvers = new WeakRefs<IUriPlacementResolver>();

        private readonly Dictionary<UriModuleItemResolverKey, IUriModuleItemResolver> _uriModuleItemResolvers = new Dictionary<UriModuleItemResolverKey, IUriModuleItemResolver>();

        private readonly Dictionary<object, Uri> _updatedUris = new Dictionary<object, Uri>();

        public Shell(IUriResolvedObjectHolder uriResolvedObjectHolder, ShellResolveFactory shellResolveFactory)
        {
            _shellResolveFactory = shellResolveFactory ?? throw new ArgumentNullException(nameof(shellResolveFactory));
            _uriResolvedObjectHolder = uriResolvedObjectHolder ?? throw new ArgumentNullException(nameof(uriResolvedObjectHolder));
        }

        public IReadOnlyDictionary<UriModuleItemResolverKey, IUriModuleItemResolver> ModuleItemResolvers => _uriModuleItemResolvers;

        public IEnumerable<IUriPlacementResolver> PlacementResolvers => _uriPlacementResolvers.ExtractAlive();

        public void AddUriPlacementResolver(IUriPlacementResolver uriPlacementResolver)
        {
            if (uriPlacementResolver == null)
            {
                throw new ArgumentNullException(nameof(uriPlacementResolver));
            }

            _uriPlacementResolvers.Add(uriPlacementResolver);
        }

        public void AddUriModuleItemResolver(UriModuleItemResolverKey key, IUriModuleItemResolver uriModuleItemResolver)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (uriModuleItemResolver == null)
            {
                throw new ArgumentNullException(nameof(uriModuleItemResolver));
            }

            _uriModuleItemResolvers.Add(key, uriModuleItemResolver);
        }

        public IShellResolve Resolve(Uri uri, params object[] attachments)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            if (attachments == null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            return _shellResolveFactory(uri, attachments);
        }

        public bool IsResolvedOpen(object resolved)
        {
            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }

            return _uriResolvedObjectHolder.Contains(resolved);
        }

        private void CheckResolvedOpened(object resolved)
        {
            if (IsResolvedOpen(resolved))
            {
                return;
            }

            throw new ArgumentException("Object is not opened in the shell.", nameof(resolved));
        }

        public int GetResolvedId(object resolved)
        {
            CheckResolvedOpened(resolved);

            return _uriResolvedObjectHolder.GetMetadata(resolved).ResolvedId;
        }

        public Uri GetResolvedUri(object resolved)
        {
            CheckResolvedOpened(resolved);

            if (_updatedUris.TryGetValue(resolved, out var updatedUri))
            {
                return updatedUri;
            }

            return _uriResolvedObjectHolder.GetMetadata(resolved).Uri;
        }

        public void CloseResolved(object resolved)
        {
            _updatedUris.Remove(resolved);
            _uriResolvedObjectHolder.GetMetadata(resolved).Disposable.Dispose();
        }

        public ShellHyperlink TryParseHyperlink(string hyperlink, int ownerId)
        {
            if (hyperlink == null)
            {
                throw new ArgumentNullException(nameof(hyperlink));
            }

            var matches = _HyperLinkRegex.Matches(hyperlink);
            if (matches.Count == 0)
            {
                return null;
            }

            // If the given text matches hyperlink template
            // then return a hyperlink.

            var match = matches[0];
            var uri = new Uri(match.Groups[1].Value);
            var title = match.Groups[2].Value;

            if (uri.IsUriShell())
            {
                var builder = new ShellUriBuilder(uri)
                {
                    OwnerId = ownerId
                };
                uri = builder.Uri;
            }

            return new ShellHyperlink(uri, title, null);
        }

        public ShellHyperlink CreateHyperlink(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var builder = new ShellUriBuilder(uri);
            var title = builder.Parameters["title"];

            var iconParameter = builder.Parameters["icon"];
            if (!Uri.TryCreate(iconParameter, UriKind.Absolute, out var icon))
            {
                throw new ArgumentException($"Icon URI \"{iconParameter}\" must be absolute");
            }

            return new ShellHyperlink(uri, title, icon);
        }

        public void UpdateResolvedUri(object resolved, Uri newUri)
        {
            CheckResolvedOpened(resolved);

            _updatedUris[resolved] = newUri ?? throw new ArgumentNullException(nameof(newUri));
        }
    }
}
