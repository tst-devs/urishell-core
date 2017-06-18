using System;

namespace UriShell
{
    public partial class UriShellSettings
	{
        public static UriShellSettings Instance { get; private set; } = new UriShellSettings(new Builder());

		private UriShellSettings(Builder builder)
		{
			Scheme = builder.Scheme;
		}

		public static void Initialize(Action<UriShellSettings.Builder> initialize)
		{
            var builder = new Builder();
			initialize(builder);
			Instance = new UriShellSettings(builder);
		}

		public string Scheme { get; }
	}
}
