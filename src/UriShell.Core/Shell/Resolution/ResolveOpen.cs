using System;
using System.Collections.Generic;
using System.Linq;

using UriShell.Shell.Registration;
using Microsoft.Extensions.Logging;

namespace UriShell.Shell.Resolution
{
    public sealed class ResolveOpen : IShellResolve
	{
        private readonly Uri _unresolvedUri;

        private readonly object[] _attachments;

        private readonly IResolveSetupFactory _resolveSetupFactory;

        private readonly IUriResolvedObjectHolder _uriResolvedObjectHolder;

        private readonly IUriResolutionCustomization _uriResolutionCustomization;

        private ResolveSetupPlayer _resolveSetupPlayer;

        private readonly IUriDisconnectTable _uriDisconnectTable;

        private readonly ILogger _logger;

        public ResolveOpen(
            Uri uri,
            object[] attachments,
            IResolveSetupFactory resolveSetupFactory,
            IUriResolvedObjectHolder uriResolvedObjectHolder,
            IUriDisconnectTable uriDisconnectTable,
            IUriResolutionCustomization uriResolutionCustomization,
            ILogger logger)
        {
            _unresolvedUri = uri ?? throw new ArgumentNullException(nameof(uri));
            _attachments = attachments ?? throw new ArgumentNullException(nameof(attachments));
            _resolveSetupFactory = resolveSetupFactory ?? throw new ArgumentNullException(nameof(resolveSetupFactory));
            _uriResolvedObjectHolder = uriResolvedObjectHolder ?? throw new ArgumentNullException(nameof(uriResolvedObjectHolder));
            _uriResolutionCustomization = uriResolutionCustomization ?? throw new ArgumentNullException(nameof(uriResolutionCustomization));
            _uriDisconnectTable = uriDisconnectTable ?? throw new ArgumentNullException(nameof(uriDisconnectTable));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IShellResolveSetup<TResolved> Setup<TResolved>() =>
            _resolveSetupFactory.Create<TResolved>(new ResolveSetupArgs(this, ReceiveResolveSetupPlayer));

        private void ReceiveResolveSetupPlayer(ResolveSetupPlayer player)
        {
            if (_resolveSetupPlayer != null)
            {
                throw new InvalidOperationException(
                    $"Setup of the object, resolved by URI \"{_unresolvedUri}\", has been already done");
            }

            _resolveSetupPlayer = player ?? throw new ArgumentNullException(nameof(player));
        }

        private (Uri uri, UriAttachmentSelector attachmentSelector) EmbedAttachments()
        {
            if (_attachments.Length == 0)
            {
                return (_unresolvedUri, id => null);
            }

            var attachmentDictionary = new Dictionary<object, object>(_attachments.Length);
            var idGenerator = new Random();

            // Replace parameter values in the URI with identifiers.
            var uriBuilder = new ShellUriBuilder(_unresolvedUri);
            foreach (var param in uriBuilder.Parameters.ToList())
            {
                var value = param.Value;

                // Detect object's placeholder by the string "{index}",
                // where index is an integer from the attachment range.

                if (value.Length <= 2)
                {
                    continue;
                }
                if (value[0] != '{')
                {
                    continue;
                }
                if (value[value.Length - 1] != '}')
                {
                    continue;
                }

                value = value.Substring(1, value.Length - 2);

                if (int.TryParse(value, out int attachmentIndex))
                {
                    if (attachmentIndex >= _attachments.Length)
                    {
                        continue;
                    }

                    value = idGenerator.Next().ToString();
                    attachmentDictionary[value] = _attachments[attachmentIndex];
                }

                uriBuilder.Parameters[param.Key] = value;
            }

            return (
                uriBuilder.Uri,
                (id) => id != null && attachmentDictionary.TryGetValue(id, out var attachment) ? attachment : null);
        }

        private object ResolveModuleItem(Uri uri, UriAttachmentSelector attachmentSelector)
        {
            var builder = new ShellUriBuilder(uri);
            var moduleItemResolverKey = new UriModuleItemResolverKey(builder.Module, builder.Item);

            if (_uriResolutionCustomization.ModuleItemResolvers.TryGetValue(moduleItemResolverKey, out var resolver))
            {
                return resolver.Resolve(uri, attachmentSelector);
            }

			throw new UriResolutionException(
                uri, $"Can't resolve URI, because a proper {nameof(IUriModuleItemResolver)} has not been registered.");
        }

        private IUriPlacementConnector ResolvePlacement(Uri uri, UriAttachmentSelector attachmentSelector, object resolved)
        {
            var placementConnector = _uriResolutionCustomization
                .PlacementResolvers
                .Select(r => r.Resolve(resolved, uri, attachmentSelector))
                .FirstOrDefault(c => c != null);

            if (placementConnector != null)
            {
                return placementConnector;
            }

            throw new UriResolutionException(
                uri, $"Can't place resolved object, because none of {nameof(IUriPlacementResolver)} didn't accept URI.");
        }

        private void Connect(Uri uri, object resolved, IUriPlacementConnector placementConnector, ShellDisposable disposable)
        {
            placementConnector.Connect(resolved);
            try
            {
                _uriResolvedObjectHolder.Add(resolved, new UriResolvedMetadata(uri, disposable));
                _uriDisconnectTable[resolved] = placementConnector;
            }
            catch (Exception)
            {
                // When failed to add object to the holder - disconnect it from UI.
                placementConnector.Disconnect(resolved);

                throw;
            }
        }

        private void PlaySetup(Uri uri, object resolved, ShellDisposable disposable)
        {
            if (_resolveSetupPlayer == null)
            {
                return;
            }

            try
            {
                _resolveSetupPlayer(uri, resolved, _logger, disposable);
            }
            catch (Exception ex)
            {
                // Setup failures should not affect opening.
                _logger.LogError(0, ex, "Error during setup");
            }
        }

        private static void DeferClose(
            object resolved,
            IUriResolvedObjectHolder uriResolvedObjectHolder,
            IUriDisconnectTable uriDisconnectTable,
            ILogger logger,
            ShellDisposable disposable)
        {
            disposable.Append(() =>
            {
                try
                {
                    uriDisconnectTable[resolved].Disconnect(resolved);

                    if (resolved is IDisposable disposableResolved)
                    {
                        disposableResolved.Dispose();
                    }

                    uriDisconnectTable.Remove(resolved);
                    uriResolvedObjectHolder.Remove(resolved);
                }
                catch (Exception ex)
                {
                    logger.LogError(0, ex, "Error when closing the view");
                }
            });
        }

        private void SendRefresh(object resolved, IUriPlacementConnector placementConnector)
        {
            if (placementConnector.IsResponsibleForRefresh)
            {
                return;
            }

            if (resolved is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }

        public IDisposable Open()
        {
            var disposable = new ShellDisposable();
            try
            {
                InternalOpen(disposable);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Error when opening the view");
            }

            return disposable;
        }

        public IDisposable OpenOrThrow()
        {
            var disposable = new ShellDisposable();
            InternalOpen(disposable);
            return disposable;
        }

        private void InternalOpen(ShellDisposable disposable)
        {
            var (uri, attachmentSelector) = EmbedAttachments();

            var resolved = ResolveModuleItem(uri, attachmentSelector);
            var placementConnector = ResolvePlacement(uri, attachmentSelector, resolved);

            Connect(uri, resolved, placementConnector, disposable);
            PlaySetup(uri, resolved, disposable);
            DeferClose(resolved, _uriResolvedObjectHolder, _uriDisconnectTable, _logger, disposable);

            _logger.LogInformation("URI \"{0}\" has been opened", uri);

            SendRefresh(resolved, placementConnector);
        }
    }
}