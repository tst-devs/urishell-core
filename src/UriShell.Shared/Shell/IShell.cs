using System;
using UriShell.Shell.Registration;

namespace UriShell.Shell
{
    public interface IShell
    {
        void AddUriPlacementResolver(IUriPlacementResolver uriPlacementResolver);

        void AddUriModuleItemResolver(UriModuleItemResolverKey key, IUriModuleItemResolver uriModuleItemResolver);

        IShellResolve Resolve(Uri uri, params object[] attachments);

        bool IsResolvedOpen(object resolved);

        int GetResolvedId(object resolved);

        Uri GetResolvedUri(object resolved);

        void CloseResolved(object resolved);

        ShellHyperlink TryParseHyperlink(string hyperlink, int ownerId);

        ShellHyperlink CreateHyperlink(Uri uri);

        void UpdateResolvedUri(object resolved, Uri newUri);
    }
}
