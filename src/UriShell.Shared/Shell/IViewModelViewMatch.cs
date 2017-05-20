using System;

namespace UriShell.Shell
{
    public interface IViewModelViewMatch
    {
        object View { get; }

        bool SupportsModelChange { get; }

        bool IsMatchToModel(object viewModel);

        void ChangeModel(object viewModel);
    }
}
