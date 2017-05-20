using System;
using System.Collections.Generic;
using System.Text;

namespace UriShell.Shell
{
    public sealed partial class ShellUriBuilder
    {
        public const int MinResolvedId = ushort.MinValue;

        public static readonly int MaxResolvedId = ushort.MaxValue;

        private string _placement = string.Empty;

        private int _ownerId;

        private string _module = string.Empty;

        private string _item = string.Empty;

        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

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
					Settings.Instance.Scheme,
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

        public int OwnerId
        {
            get
            {
                return _ownerId;
            }
            set
            {
                CheckOwnerId(value);
                _ownerId = value;
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

        public IDictionary<string, string> Parameters
        {
            get
            {
                return _parameters;
            }
		}

        internal static void CheckOwnerId(int ownerId)
        {
			if (ownerId >= MinResolvedId && ownerId <= MaxResolvedId)
			{
                return;
			}

            throw new ArgumentOutOfRangeException(nameof(OwnerId));
        }

		private void ParametersFromUriQuery(string query)
		{
			if (query.Length == 0)
			{
				return;
			}

			var i = 1;

			while (i < query.Length)
			{
				var si = i;
				var ti = -1;

				while (i < query.Length)
				{
					var ch = query[i];

					if (ch == '&')
					{
						break;
					}

					if (ch == '=' && ti < 0)
					{
						ti = i;
					}

					i++;
				}

				string name = null;
				string value;

				if (ti >= 0)
				{
					name = query.Substring(si, ti - si);
					value = query.Substring(ti + 1, i - ti - 1);
				}
				else
				{
					value = query.Substring(si, i - si);
				}

				_parameters.Add(name, Uri.UnescapeDataString(value));

				i++;
			}
		}

		private string ParametersToUriQuery()
		{
			if (_parameters.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			foreach (string key in _parameters.Keys)
			{
				if (sb.Length > 0)
				{
					sb.Append('&');
				}
				sb.AppendFormat("{0}={1}", key, Uri.EscapeDataString(_parameters[key]));
			}

			return sb.ToString();
		}
    }
}