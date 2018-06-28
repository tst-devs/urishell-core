using System;
using System.Collections.Generic;
using System.Text;

namespace UriShell.Shell
{
    public sealed partial class ShellUriBuilder
    {
        private string _placement = string.Empty;

        private string _module = string.Empty;

        private string _item = string.Empty;

        public ShellUriBuilder()
        {
        }

        public ShellUriBuilder(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            Placement = uri.Host;
            if (!uri.IsDefaultPort)
            {
                OwnerId = uri.Port;
            }

            var path = uri.AbsolutePath.Substring(1);
            var moduleItemDelim = path.IndexOf('/');
            if (moduleItemDelim >= 0)
            {
                Module = path.Substring(0, moduleItemDelim);
                Item = path.Substring(moduleItemDelim + 1).TrimEnd('/');
            }

            ParametersFromUriQuery(uri.Query);
        }

        public static Writer StartUri()
        {
            return new Writer();
        }

        public Uri Uri
        {
            get
            {
                var ub = new UriBuilder(
                    UriShellSettings.Scheme,
                    Placement,
                    OwnerId,
                    $"/{Module}/{Item}");
                ub.Query = ParametersToUriQuery();

                return ub.Uri;
            }
        }

        public string Placement
        {
            get
            {
                return _placement;
            }
            set
            {
                _placement = value ?? string.Empty;
            }
        }

        public string Module
        {
            get
            {
                return _module;
            }
            set
            {
                _module = value ?? string.Empty;
            }
        }

        public string Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value ?? string.Empty;
            }
        }

        public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public int OwnerId { get; set; }

        private void ParametersFromUriQuery(string query)
        {
            if (string.IsNullOrEmpty(query) || query == "?")
            {
                return;
            }

            var scanIndex = 0;
            if (query[0] == '?')
            {
                scanIndex = 1;
            }

            var textLength = query.Length;
            var equalIndex = query.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }

            while (scanIndex < textLength)
            {
                var delimiterIndex = query.IndexOf('&', scanIndex);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }

                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(query[scanIndex]))
                    {
                        scanIndex++;
                    }

                    var name = query
                        .Substring(scanIndex, equalIndex - scanIndex)
                        .Replace('+', ' ');
                    var value = query
                        .Substring(equalIndex + 1, delimiterIndex - equalIndex - 1)
                        .Replace('+', ' ');

                    Parameters[Uri.UnescapeDataString(name)] = Uri.UnescapeDataString(value);

                    equalIndex = query.IndexOf('=', delimiterIndex);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                else
                {
                    if (delimiterIndex > scanIndex)
                    {
                        Parameters[query.Substring(scanIndex, delimiterIndex - scanIndex)] = string.Empty;
                    }
                }

                scanIndex = delimiterIndex + 1;
            }
        }

        private string ParametersToUriQuery()
        {
            if (Parameters.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var p in Parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }
                sb.Append(string.Concat(p.Key, "=", Uri.EscapeDataString(p.Value)));
            }

            return sb.ToString();
        }
    }
}