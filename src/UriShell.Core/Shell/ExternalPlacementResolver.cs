using System;
using System.ComponentModel;
using System.Diagnostics;

namespace UriShell.Shell
{
    public sealed class ExternalPlacementResolver : IUriPlacementResolver, IUriPlacementConnector
    {
        public IUriPlacementConnector Resolve(object resolved, Uri uri, UriAttachmentSelector attachmentSelector)
        {
            var builder = new ShellUriBuilder(uri);
            if (builder.Placement == "external" && resolved is Process)
            {
                return this;
            }

            return null;
        }

        public void Connect(object resolved)
        {
            var process = (Process)resolved;
            try
            {
                process.Start();
            }
            catch (Win32Exception ex)
            {
                var message = $"Error while opening the file \"{process.StartInfo.FileName}\": {ex.Message}";
                throw new Exception(message, ex);
            }
        }

        public void Disconnect(object resolved) => ((Process)resolved).Kill();

        public bool IsResponsibleForRefresh => true;
    }
}
