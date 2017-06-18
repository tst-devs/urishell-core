using Xunit;
using NSubstitute;

namespace UriShell.Shell
{
	public class ShellExtensionsTest
	{
        [Fact]
		public void ClosesResolvedListUsingShellCloseResolvedForEachResolved()
		{
			var view1 = new object();
			var view2 = new object();

			var shell = Substitute.For<IShell>();
			ShellExtensions.CloseResolvedList(shell, new object[] { view1, view2 });

			shell.Received(1).CloseResolved(view1);
			shell.Received(1).CloseResolved(view2);
		}
	}
}
