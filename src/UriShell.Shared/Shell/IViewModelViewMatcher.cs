using System;

namespace UriShell.Shell
{
    public interface IViewModelViewMatcher
    {
        IViewModelViewMatch Match(object viewModel);
    }
}
