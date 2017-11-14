using FluentAssertions;
using Xunit;

namespace UriShell.Shell
{
    public class ShellDisposableTest
    {
        [Fact]
        public void AppendReturnsTheSameInstance()
        {
            var disposable = new ShellDisposable();
            var disposable1 = disposable.Append(() => { });
            var disposable2 = disposable1.Append(() => { });

            disposable2.Should().BeSameAs(disposable);
            disposable1.Should().BeSameAs(disposable);
        }

        [Fact]
        public void InvokesActionsFromCtor()
        {
            var invoked = false;

            var disposable = new ShellDisposable(() => invoked = true);
            disposable.Dispose();

            invoked.Should().BeTrue();
        }

        [Fact]
        public void InvokesActionsFromAppend()
        {
            var invoked1 = false;
            var invoked2 = false;

            new ShellDisposable()
                .Append(() => invoked1 = true)
                .Append(() => invoked2 = true)
                .Dispose();

            invoked1.Should().BeTrue();
            invoked2.Should().BeTrue();
        }

        [Fact]
        public void DoesntInvokesActionFromAppendTwice()
        {
            var invokeCount1 = 0;
            var invokeCount2 = 0;

            var disposable = new ShellDisposable()
                .Append(() => invokeCount1++)
                .Append(() => invokeCount2++);
            disposable.Dispose();
            disposable.Dispose();

            invokeCount1.Should().Be(1);
            invokeCount2.Should().Be(1);
        }
    }
}
