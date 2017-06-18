using System;
using System.Linq;
using System.Reflection;

namespace UriShell.Shell
{
    public sealed class ViewModelPropertyMatch : IViewModelViewMatch
    {
        public static ViewModelPropertyMatch TryMatch(object viewModel, Type viewType, Func<object> viewFactory)
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

            var viewModelProperty = viewType
                .GetTypeInfo()
                .DeclaredProperties
                .Where(pi => pi.IsDefined(typeof(ViewModelAttribute), false))
                .FirstOrDefault(pi => IsPropertyMatchToModel(pi, viewModel));

            if (viewModelProperty == null)
            {
                return null;
            }

            var match = new ViewModelPropertyMatch(viewFactory(), viewModelProperty);
            match.ChangeModel(viewModel);

            return match;
        }

        private static bool IsPropertyMatchToModel(PropertyInfo viewModelProperty, object viewModel)
        {
            if (!viewModelProperty.CanWrite)
            {
                return false;
            }

            var propertyTypeInfo = viewModelProperty.PropertyType.GetTypeInfo();
            var viewModelTypeInfo = viewModel.GetType().GetTypeInfo();

            return propertyTypeInfo.IsAssignableFrom(viewModelTypeInfo);
        }

        private readonly PropertyInfo _viewModelProperty;

        private ViewModelPropertyMatch(object view, PropertyInfo viewModelProperty)
        {
            View = view;
            _viewModelProperty = viewModelProperty;
        }

        public object View { get; }

        public bool SupportsModelChange => true;

        public bool IsMatchToModel(object viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            return IsPropertyMatchToModel(_viewModelProperty, viewModel);
        }

        public void ChangeModel(object viewModel)
        {
            if (!IsMatchToModel(viewModel))
            {
                throw new ArgumentException("Incompatible view model", nameof(viewModel));
            }

            _viewModelProperty.SetValue(View, viewModel, new object[0]);
        }
    }
}
