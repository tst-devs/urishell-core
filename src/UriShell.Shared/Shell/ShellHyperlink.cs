using System;

namespace UriShell.Shell
{
	public class ShellHyperlink
	{
        private readonly string _text;

		public ShellHyperlink(Uri uri)
			: this(uri, null, null)
		{
		}

		public ShellHyperlink(Uri uri, string text)
			: this(uri, text, null)
		{
		}

		public ShellHyperlink(Uri uri, string text, Uri icon)
		{
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Icon = icon;
			_text = text;
		}

		public Uri Uri { get; }

		public Uri Icon { get; }

		public string Text
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_text))
				{
					return Uri.ToString();
				}

				return _text;
			}
		}
	}
}
