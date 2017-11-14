using System.Collections.Generic;

namespace UriShell.Shell.Resolution
{
    public interface IUriResolutionCustomization
    {
        IReadOnlyDictionary<UriModuleItemResolverKey, IUriModuleItemResolver> ModuleItemResolvers { get; }

        IEnumerable<IUriPlacementResolver> PlacementResolvers { get; }
    }
}