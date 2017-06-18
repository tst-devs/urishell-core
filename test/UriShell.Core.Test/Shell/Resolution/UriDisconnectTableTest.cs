using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace UriShell.Shell.Resolution
{
	public class UriDisconnectTableTest
	{
        [Fact]
		public void SetsPlacementConnectorForResolved()
		{
			var resolved = new object();

			var shell = Substitute.For<IShell>();
			shell.IsResolvedOpen(resolved).Returns(true);

			var table = new UriDisconnectTable();
			var connector = Substitute.For<IUriPlacementConnector>();

			table[resolved] = connector;

			var connectorFromTable = table[resolved];

            connectorFromTable.Should().BeSameAs(connector);
		}

        [Fact]
		public void ThrowsExceptionWhenGettingPlacementConnectorAfterRemove()
		{
			var resolved = new object();

			var shell = Substitute.For<IShell>();
			shell.IsResolvedOpen(resolved).Returns(true);

			var table = new UriDisconnectTable();
			var connector = Substitute.For<IUriPlacementConnector>();

			table[resolved] = connector;
			table.Remove(resolved);

            Action getFromTable = () => connector = table[resolved];

            getFromTable.ShouldThrow<KeyNotFoundException>()
                        .WithMessage($"Entry of {resolved} hasn't been found.");
		}

        [Fact]
		public void ThrowsExceptionWhenGettingPlacementConnectorForUnknownResolved()
		{
			var table = new UriDisconnectTable();
			IUriPlacementConnector connector = null;

			var resolved = "unknown123";
			Action getFromTable = () => connector = table[resolved];

			getFromTable.ShouldThrow<KeyNotFoundException>()
						.WithMessage($"Entry of {resolved} hasn't been found.");
		}

        [Fact]
        public void ThrowsExceptionWhenRemovingUnknownResolved()
        {
            var table = new UriDisconnectTable();

            var resolved = "unknown456";

            table.Invoking(t => t.Remove(resolved))
                 .ShouldThrow<KeyNotFoundException>()
                 .WithMessage($"Entry of {resolved} hasn't been found.");
        }
	}
}
