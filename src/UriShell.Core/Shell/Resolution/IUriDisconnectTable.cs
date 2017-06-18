namespace UriShell.Shell.Resolution
{
	public interface IUriDisconnectTable
	{
		IUriPlacementConnector this[object resolved] { get; set; }

		void Remove(object resolved);
	}
}
