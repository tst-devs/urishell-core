using System;

namespace UriShell.Shell
{
    partial class ShellUriBuilder
    {
        public sealed class Writer
        {
            private readonly ShellUriBuilder _builder = new ShellUriBuilder();

            internal Writer()
            {
            }

            public Uri End()
            {
                return _builder.Uri;
            }

            public Writer Placement(string placement)
            {
                _builder.Placement = placement;
                return this;
            }

            public Writer OwnerId(int ownerId)
            {
                _builder.OwnerId = ownerId;
                return this;
            }

            public Writer Module(string module)
            {
                _builder.Module = module;
                return this;
            }

            public Writer Item(string item)
            {
                _builder.Item = item;
                return this;
            }

            public Writer Parameter(string name, string value)
            {
                _builder.Parameters[name] = value;
                return this;
            }

            public Writer Attachment(string name, int index)
            {
                var indexStr = index.ToString();
                _builder.Parameters[name] = string.Concat("{", indexStr, "}");
                return this;
            }
        }
    }
}