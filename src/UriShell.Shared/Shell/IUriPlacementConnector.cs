namespace UriShell.Shell
{
    public interface IUriPlacementConnector
    {
        void Connect(object resolved);

        void Disconnect(object resolved);

        bool IsResponsibleForRefresh { get; }
    }
}
