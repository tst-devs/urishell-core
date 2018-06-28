using System;
using System.Collections.Generic;
using System.Linq;

namespace UriShell.Shell
{
    partial class Shell
    {
        private sealed class WeakRefs<T> where T : class
        {
            private readonly List<WeakReference> _list = new List<WeakReference>();

            public void Add(T item)
            {
                if (ExtractAlive().Contains(item))
                {
                    return;
                }

                _list.Add(new WeakReference(item));
            }

            public IReadOnlyList<T> ExtractAlive()
            {
                var alive = new List<T>(_list.Count);
                for (int i = _list.Count - 1; i >= 0; i--)
                {
                    if (_list[i].Target is T target)
                    {
                        alive.Add(target);
                    }
                    else
                    {
                        _list.RemoveAt(i);
                    }
                }

                return alive;
            }
        }
    }
}