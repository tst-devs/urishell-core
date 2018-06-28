using System;
using System.Collections.Generic;

namespace UriShell.Shell.Resolution
{
    public sealed class UriDisconnectTable : IUriDisconnectTable
    {
        private readonly Dictionary<object, IUriPlacementConnector> _connectors = new Dictionary<object, IUriPlacementConnector>();

        public IUriPlacementConnector this[object resolved]
        {
            get
            {
                if (resolved == null)
                {
                    throw new ArgumentNullException(nameof(resolved));
                }

                if (_connectors.TryGetValue(resolved, out var connector))
                {
                    return connector;
                }

                throw ResolvedNotFoundException(resolved);
            }
            set
            {
                if (resolved == null)
                {
                    throw new ArgumentNullException(nameof(resolved));
                }

                _connectors[resolved] = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public void Remove(object resolved)
        {
            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }

            if (!_connectors.Remove(resolved))
            {
                throw ResolvedNotFoundException(resolved);
            }
        }

        private static Exception ResolvedNotFoundException(object resolved)
        {
            return new KeyNotFoundException($"Entry of {resolved} hasn't been found.");
        }
    }
}
