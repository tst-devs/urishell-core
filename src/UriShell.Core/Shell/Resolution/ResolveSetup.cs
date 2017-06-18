using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UriShell.Shell.Registration;

namespace UriShell.Shell.Resolution
{
    public sealed class ResolveSetup<TResolved> : IShellResolveSetup<TResolved>
    {
        private readonly IShellResolveOpen _resolveOpen;

        private readonly Action<ResolveSetupPlayer> _playerSender;

        private Action<TResolved> _onReady;

        private Action<TResolved> _onFinished;

		public ResolveSetup(ResolveSetupArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            _resolveOpen = args.ResolveOpen;
            _playerSender = args.PlayerSender;
        }

        public IShellResolveSetup<TResolved> OnReady(Action<TResolved> action)
        {
            _onReady = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public IShellResolveSetup<TResolved> OnFinished(Action<TResolved> action)
        {
            _onFinished = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public IDisposable Open() => SendPlayerAndInvoke(_ => _.Open());

        public IDisposable OpenOrThrow() => SendPlayerAndInvoke(_ => _.OpenOrThrow());

        private IDisposable SendPlayerAndInvoke(Func<IShellResolveOpen, IDisposable> launchAction)
        {
            var player = CreatePlayer(_onReady, _onFinished);
            if (player != null)
            {
                _playerSender(player);
            }

            return launchAction(_resolveOpen);
        }

        private static ResolveSetupPlayer CreatePlayer(Action<TResolved> onReady, Action<TResolved> onFinished)
        {
            if (onReady == null && onFinished == null)
            {
                return null;
            }

            return (uri, resolved, logger, disposable) =>
                {
                    if (!TryCastResolved(uri, resolved, logger, out var cast))
                    {
                        return;
                    }

                    onReady?.Invoke(cast);

                    if (onFinished != null)
                    {
                        disposable.Append(() => onFinished(cast));
                    }
                };
        }

        private static bool TryCastResolved(Uri uri, object resolved, ILogger logger, out TResolved cast)
        {
            var resolvedTypeInfo = resolved?.GetType().GetTypeInfo();
            var expectedTypeInfo = typeof(TResolved).GetTypeInfo();

            if (expectedTypeInfo.IsAssignableFrom(resolvedTypeInfo))
            {
                cast = (TResolved)resolved;
                return true;
            };

            logger.LogWarning(
                "Setup of the object, resolved via URI \"{0}\", wasn't called due to type mismatch. Expected {1}, actual {2}.",
                uri, typeof(TResolved).Name, resolvedTypeInfo?.Name ?? "(null)");

            cast = default(TResolved);
			return false;
        }
    }
}
