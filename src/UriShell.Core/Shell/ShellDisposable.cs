using System;
using System.Collections.Generic;

namespace UriShell.Shell
{
    public sealed class ShellDisposable : IDisposable
    {
        private List<Action> _actions;

        public ShellDisposable()
        {
        }

        public ShellDisposable(Action dispose)
        {
            Append(dispose);
        }

        public ShellDisposable Append(Action dispose)
        {
            if (_actions == null)
            {
                _actions = new List<Action>();
            }

            _actions.Add(dispose);
            return this;
        }

        public void Dispose()
        {
            if (_actions == null || _actions.Count == 0)
            {
                return;
            }

            _actions.ForEach(dispose => dispose());
            _actions.Clear();
        }
    }
}