using System;
using System.Collections.Generic;
using System.Linq;

using UriShell.Shell.Resolution;

namespace UriShell.Shell.Connectors
{
    public sealed class ConnectedDragDrop : IConnectedDragDrop, IUriPlacementConnector
    {
        private readonly IUriDisconnectTable _uriDisconnectTable;

        private readonly Dictionary<object, object> _data = new Dictionary<object, object>();

        private object _connected;

        public ConnectedDragDrop(IUriDisconnectTable uriDisconnectTable)
        {
            _uriDisconnectTable = uriDisconnectTable ?? throw new ArgumentNullException(nameof(uriDisconnectTable));
        }

        public bool IsActive => _connected != null;

        private void CheckActiveIs(bool required)
        {
            if (IsActive != required)
            {
                throw new InvalidOperationException(
                    IsActive ? "Drag operation is in progress."
                    : "There is no active drag operation.");
            }
        }

        public void Drag(object connected)
        {
            CheckActiveIs(false);

            _connected = connected ?? throw new ArgumentNullException(nameof(connected));

            _uriDisconnectTable[connected].Disconnect(connected);
            _uriDisconnectTable[connected] = this;
        }

        public void Drop(IUriPlacementConnector target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            CheckActiveIs(true);

			var dragged = _connected;

            target.Connect(dragged);
            _uriDisconnectTable[dragged] = target;

            _connected = null;
            _data.Clear();
        }

        public bool IsDragging(object resolved)
        {
            return resolved == _connected;
        }

        public void SetData<TFormat>(ConnectedDragDropKey<TFormat> key, TFormat data)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CheckActiveIs(true);

            _data[key] = data;
        }

        public TFormat GetData<TFormat>(ConnectedDragDropKey<TFormat> key)
        {
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			CheckActiveIs(true);
			
            if (_data.TryGetValue(key, out var data)) 
            {
                return (TFormat)data;
            }

            return default(TFormat);
        }

        public bool GetDataPresent<TFormat>(ConnectedDragDropKey<TFormat> key)
        {
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			return _data.ContainsKey(key);
        }

        void IUriPlacementConnector.Connect(object resolved)
        {
            // No one has to use ConnectorDragDrop as a connector. 
            throw new NotImplementedException();
        }

        void IUriPlacementConnector.Disconnect(object resolved)
        {
            DraggedClosed?.Invoke(this, EventArgs.Empty);

            // The call of Disconnect means that the shell closes the object
            // during drag. In this case, ConnectedDragDrop owns the object 
            // hence is responsible for disposing stored data.
            foreach (var disposable in _data.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            _connected = null;
            _data.Clear();
        }

        bool IUriPlacementConnector.IsResponsibleForRefresh => false;

        public event EventHandler DraggedClosed;
    }
}