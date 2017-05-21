using System;

namespace UriShell.Shell.Connectors
{
	public interface IConnectedDragDrop
	{
		bool IsActive { get; }

		void Drag(object connected);

		void Drop(IUriPlacementConnector target);

		bool IsDragging(object resolved);

		void SetData<TFormat>(ConnectedDragDropKey<TFormat> key, TFormat data);

		TFormat GetData<TFormat>(ConnectedDragDropKey<TFormat> key);

		bool GetDataPresent<TFormat>(ConnectedDragDropKey<TFormat> key);

		event EventHandler DraggedClosed;
	}
}