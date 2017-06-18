using System;
using System.Collections;
using FluentAssertions;
using NSubstitute;
using UriShell.Shell.Resolution;
using Xunit;

namespace UriShell.Shell.Connectors
{
    public class ConnectedDragDropTest
    {
        private readonly IUriDisconnectTable _uriDisconnectTable = Substitute.For<IUriDisconnectTable>();

        [Fact]
        public void IsDraggingReturnsTrueAfterDrag()
        {
            var connected = new object();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            dragDrop.IsDragging(connected).Should().BeTrue();
            dragDrop.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsDraggingReturnsFalseAfterDrop()
        {
            var connected = new object();
            var connector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);
            dragDrop.Drop(connector);

            dragDrop.IsDragging(connected).Should().BeFalse();
            dragDrop.IsActive.Should().BeFalse();
        }

        [Fact]
        public void IsDraggingReturnsFalseAfterDisconnect()
        {
            var connected = new object();
            var connector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            IUriPlacementConnector dragDropConnector = dragDrop;
            dragDropConnector.Disconnect(connected);

            dragDrop.IsDragging(connected).Should().BeFalse();
            dragDrop.IsActive.Should().BeFalse();
        }

        [Fact]
        public void IsDraggingReturnsFalseForUnkownObject()
        {
            var connected = new object();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            dragDrop.IsDragging(new object()).Should().BeFalse();
            dragDrop.IsDragging(new object()).Should().BeFalse();
        }

        [Fact]
        public void DisconnectsFromUriDisconnectTableEntryOnDrag()
        {
            var connected = new object();
            var sourceConnector = Substitute.For<IUriPlacementConnector>();

            _uriDisconnectTable[connected].Returns(sourceConnector);

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            sourceConnector.Received(1).Disconnect(connected);
        }

        [Fact]
        public void IsDraggingReturnsTrueBeforeDisconnectFromUriDisconnectTableEntry()
        {
            var connected = new object();
            var sourceConnector = Substitute.For<IUriPlacementConnector>();

            _uriDisconnectTable[connected].Returns(sourceConnector);

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);

            sourceConnector
                .When(_ => _.Disconnect(connected))
                .Do(args => dragDrop.IsDragging(args[0]).Should().BeTrue());

            dragDrop.Drag(connected);
        }

        [Fact]
        public void RegistersInUriDisconnectTableAfterDisconnectFromSourcePlacementConnector()
        {
            var connected = new object();
            var sourceConnector = Substitute.For<IUriPlacementConnector>();

            _uriDisconnectTable[connected].Returns(sourceConnector);

            sourceConnector
                .When(_ => _.Disconnect(connected))
                .Do(_ => _uriDisconnectTable.DidNotReceiveWithAnyArgs()[connected] = null);

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            _uriDisconnectTable.Received(1)[connected] = dragDrop;
        }

        [Fact]
        public void ConnectsToTargetPlacementConnectorOnDrop()
        {
            var connected = new object();
            var targetConnector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);
            dragDrop.Drop(targetConnector);

            targetConnector.Received(1).Connect(connected);
        }

        [Fact]
        public void IsDraggingReturnsTrueBeforeConnectToTargetPlacementConnector()
        {
            var connected = new object();
            var targetConnector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            targetConnector
                .When(_ => _.Connect(connected))
                .Do(args => dragDrop.IsDragging(args[0]).Should().BeTrue());

            dragDrop.Drop(targetConnector);
        }

        [Fact]
        public void RegistersTargetConnectorInUriDisconnectTableOnDrop()
        {
            var connected = new object();
            var targetConnector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);
            dragDrop.Drop(targetConnector);

            _uriDisconnectTable.Received(1)[connected] = targetConnector;
        }

        [Fact]
        public void SetsMultipleFormatData()
        {
            var data1 = new Hashtable();
            var data2 = new ArrayList();

            var connected = new object();

            var key1 = new ConnectedDragDropKey<Hashtable>();
            var key2 = new ConnectedDragDropKey<ArrayList>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            dragDrop.SetData(key1, data1);
            dragDrop.SetData(key2, data2);

            dragDrop.GetData(key1).Should().BeSameAs(data1);
            dragDrop.GetData(key2).Should().BeSameAs(data2);
        }

        [Fact]
        public void ThrowsExceptionWhenSettingDataWhileNotDragging()
        {
            var key = new ConnectedDragDropKey<Type>();
            var data = typeof(ConnectedDragDropTest);

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);

            dragDrop.Invoking(d => d.SetData(key, data))
                    .ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionWhenGettingDataForResolvedNotConnected()
        {
            var key = new ConnectedDragDropKey<Type>();
            var data = typeof(ConnectedDragDropTest);

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);

            dragDrop.Invoking(d => d.GetData(key))
                    .ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ReturnsDataPresentForDataHasBeenSet()
        {
            var data1 = new Hashtable();
            var data2 = new ArrayList();

            var connected = new object();

            var key1 = new ConnectedDragDropKey<Hashtable>();
            var key2 = new ConnectedDragDropKey<ArrayList>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            dragDrop.SetData(key1, data1);
            dragDrop.SetData(key2, data2);

            dragDrop.GetDataPresent(key1).Should().BeTrue();
            dragDrop.GetDataPresent(key2).Should().BeTrue();
        }

        [Fact]
        public void ReturnsDataNotPresentForDataHasNotBeenSet()
        {
            var data1 = new Hashtable();
            var data2 = new ArrayList();

            var connected = new object();

            var key1 = new ConnectedDragDropKey<Hashtable>();
            var key2 = new ConnectedDragDropKey<ArrayList>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            dragDrop.SetData(key1, data1);

            dragDrop.GetDataPresent(key2).Should().BeFalse();
        }

        [Fact]
        public void DisposesDataOnDisconnect()
        {
            var connected = new object();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            var key = new ConnectedDragDropKey<IDisposable>();
            var disposable = Substitute.For<IDisposable>();

            dragDrop.SetData(key, disposable);

            IUriPlacementConnector dragDropConnector = dragDrop;
            dragDropConnector.Disconnect(connected);

            disposable.Received(1).Dispose();
        }

        [Fact]
        public void DoNotDisposesDataOnDrop()
        {
            var connected = new object();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            var key = new ConnectedDragDropKey<IDisposable>();
            var disposable = Substitute.For<IDisposable>();

            dragDrop.SetData(key, disposable);

            var targetConnector = Substitute.For<IUriPlacementConnector>();
            dragDrop.Drop(targetConnector);

            disposable.DidNotReceive().Dispose();
        }

        [Fact]
        public void RaisesDraggedClosedOnDisconnect()
        {
            var connected = new object();
            var connector = Substitute.For<IUriPlacementConnector>();

            var dragDrop = new ConnectedDragDrop(_uriDisconnectTable);
            dragDrop.Drag(connected);

            var wasRaised = false;
            dragDrop.DraggedClosed += (sender, e) => wasRaised = true;

            IUriPlacementConnector dragDropConnector = dragDrop;
            dragDropConnector.Disconnect(connected);

            wasRaised.Should().BeTrue();
        }
    }
}
