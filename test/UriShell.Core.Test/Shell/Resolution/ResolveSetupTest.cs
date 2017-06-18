using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

using Xunit;
using FluentAssertions;

using NSubstitute;

using UriShell.Shell.Registration;

namespace UriShell.Shell.Resolution
{
    public class ResolveSetupTest
    {
        private readonly Uri _uri = new Uri("tst://placement/module/item");
        private readonly IShellResolveOpen _resolveOpen = Substitute.For<IShellResolveOpen>();
        private readonly ILogger _logger = Substitute.For<ILogger>();
        private readonly ShellDisposable _disposable = new ShellDisposable();

        [Fact]
        public void DoesntInvokeCallbackWhenSetupOnReady()
        {
            var wasCalled = false;

            var setup = new ResolveSetup<string>(
                new ResolveSetupArgs(_resolveOpen, _ => wasCalled = true));
            setup.OnReady(s => { });

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void DoesntInvokeCallbackWhenSetupOnFinished()
        {
            var wasCalled = false;

            var setup = new ResolveSetup<string>(
                new ResolveSetupArgs(_resolveOpen, _ => wasCalled = true));
            setup.OnFinished(s => { });

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void InvokesAppropriateResolveOpenMethodOnOpen()
        {
            var setup = new ResolveSetup<string>(new ResolveSetupArgs(_resolveOpen, _ => { }));
            setup.Open();

            _resolveOpen.Received(1).Open();
        }

        [Fact]
        public void InvokesAppropriateResolveOpenMethodOnOpenOrThrow()
        {
            var setup = new ResolveSetup<string>(new ResolveSetupArgs(_resolveOpen, _ => { }));
            setup.OpenOrThrow();

            _resolveOpen.Received(1).OpenOrThrow();
        }

        [Fact]
        public void InvokesOnReadyOnceWhenOpen()
        {
            object receivedInOnReady = null;

            var passedToPlayer = new object();
            var onReadyCount = 0;

            var action = new Action<object>(
                o =>
                {
                    receivedInOnReady = o;
                    onReadyCount++;
                });
            var setup = new ResolveSetup<object>(new ResolveSetupArgs(
                _resolveOpen,
                p => p(_uri, passedToPlayer, _logger, _disposable)));

            setup.OnReady(action).Open();

            onReadyCount.Should().Be(1);
            receivedInOnReady.Should().Be(passedToPlayer);
        }

        [Fact]
        public void InvokesOnReadyOnceWhenOpenOrThrow()
        {
            object receivedInOnReady = null;

            var passedToPlayer = new object();
            var onReadyCount = 0;

            var action = new Action<object>(
                o =>
                {
                    receivedInOnReady = o;
                    onReadyCount++;
                });
            var setup = new ResolveSetup<object>(new ResolveSetupArgs(
                _resolveOpen,
                p => p(_uri, passedToPlayer, _logger, _disposable)));

            setup.OnReady(action).OpenOrThrow();

            onReadyCount.Should().Be(1);
            receivedInOnReady.Should().Be(passedToPlayer);
        }

        [Fact]
        public void DoesntInvokeOnFinishedWhenOpen()
        {
            var wasCalled = false;

            var action = new Action<object>(o => wasCalled = true);
            var setup = new ResolveSetup<object>(
                new ResolveSetupArgs(_resolveOpen, p => p(_uri, new object(), _logger, _disposable)));

            setup.OnFinished(action).Open();

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void DoesntInvokeOnFinishedWhenOpenOrThrow()
        {
            var wasCalled = false;

            var action = new Action<object>(o => wasCalled = true);
            var setup = new ResolveSetup<object>(
                new ResolveSetupArgs(_resolveOpen, p => p(_uri, new object(), _logger, _disposable)));

            setup.OnFinished(action).OpenOrThrow();

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void InvokesOnFinishedOnceWhenShellDisposableDisposedAfterOpen()
        {
            object receivedInOnFinished = null;

            var passedToPlayer = new object();
            var onFinishedCount = 0;

            var action = new Action<object>(
                o =>
                {
                    receivedInOnFinished = o;
                    onFinishedCount++;
                });
            var setup = new ResolveSetup<object>(new ResolveSetupArgs(
                _resolveOpen,
                p => p(_uri, passedToPlayer, _logger, _disposable)));

            setup.OnFinished(action).Open();
            _disposable.Dispose();

            onFinishedCount.Should().Be(1);
            receivedInOnFinished.Should().Be(passedToPlayer);
        }

        [Fact]
        public void InvokesOnFinishedOnceWhenShellDisposableDisposedAfterOpenOrThrow()
        {
			object receivedInOnFinished = null;

			var passedToPlayer = new object();
			var onFinishedCount = 0;

			var action = new Action<object>(
				o =>
				{
					receivedInOnFinished = o;
					onFinishedCount++;
				});
			var setup = new ResolveSetup<object>(new ResolveSetupArgs(
				_resolveOpen,
				p => p(_uri, passedToPlayer, _logger, _disposable)));

			setup.OnFinished(action).OpenOrThrow();
			_disposable.Dispose();

			onFinishedCount.Should().Be(1);
			receivedInOnFinished.Should().Be(passedToPlayer);
        }

        [Fact]
        public void LogsWhenResolvedIncompatibleOnOpen()
        {
            var resolved = new List<int>();

            var setup = new ResolveSetup<StringBuilder>(new ResolveSetupArgs(
                _resolveOpen, p => p(_uri, resolved, _logger, _disposable)));

            var expectedTypeName = nameof(StringBuilder);
            var resolvedTypeName = resolved.GetType().Name;

            setup.OnReady(_ => { }).Open();

            _logger.Received().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(s =>
                               s.ToString().Contains(_uri.ToString())
                               && s.ToString().Contains(resolvedTypeName)
                               && s.ToString().Contains(expectedTypeName)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogsWhenResolvedIncompatibleOnOpenOrThrow()
        {
            var resolved = new List<int>();

            var setup = new ResolveSetup<StringBuilder>(new ResolveSetupArgs(
                _resolveOpen, p => p(_uri, resolved, _logger, _disposable)));

            var expectedTypeName = nameof(StringBuilder);
            var resolvedTypeName = resolved.GetType().Name;

            setup.OnReady(_ => { }).OpenOrThrow();

            _logger.Received().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(s =>
                               s.ToString().Contains(_uri.ToString())
                               && s.ToString().Contains(resolvedTypeName)
                               && s.ToString().Contains(expectedTypeName)),
                Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>());
        }
    }
}
