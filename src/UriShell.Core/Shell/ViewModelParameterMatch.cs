using System;
using System.Linq;
using System.Reflection;

namespace UriShell.Shell
{
    public sealed class ViewModelParameterMatch : IViewModelViewMatch
    {
        public static ViewModelParameterMatch TryMatch(object viewModel, Type viewType, Func<ParameterInfo, object> viewFactory)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }
            if (viewFactory == null)
            {
                throw new ArgumentNullException(nameof(viewFactory));
            }

            var viewModelParameter = viewType
                .GetTypeInfo()
                .DeclaredConstructors
                .SelectMany(c => c.GetParameters())
                .FirstOrDefault(pi => pi.IsDefined(typeof(ViewModelAttribute), false));

            if (viewModelParameter == null)
            {
                return null;
            }

            var parameterTypeInfo = viewModelParameter.ParameterType.GetTypeInfo();
            var viewModelTypeInfo = viewModel.GetType().GetTypeInfo();

            if (!parameterTypeInfo.IsAssignableFrom(viewModelTypeInfo))
            {
                return null;
            }

            return new ViewModelParameterMatch(viewFactory(viewModelParameter));
        }

        private ViewModelParameterMatch(object view)
        {
            View = view;
        }

        public object View { get; }

        public bool SupportsModelChange => false;

        public bool IsMatchToModel(object viewModel) => false;

        public void ChangeModel(object viewModel) => throw new NotSupportedException();
    }
}
