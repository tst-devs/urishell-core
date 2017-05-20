using System;

namespace UriShell
{
	public partial class Settings
	{
        public static Settings Instance { get; private set; }

		private Settings(Builder builder)
		{
			Scheme = builder.Scheme;
		}

		public static void Initialize(Action<Settings.Builder> initialize)
		{
			if (Instance != null)
            {
                throw new InvalidOperationException("UriShell settings must be initalized only once.");
            }

			var builder = new Builder();
			initialize(builder);
			Instance = new Settings(builder);
		}

		public string Scheme { get; }
	}
}
