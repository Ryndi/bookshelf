using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras
{
    public class ExistingExtraFileService : IHandle<AuthorScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly List<IImportExistingExtraFiles> _existingExtraFileImporters;
        private readonly Logger _logger;

        public ExistingExtraFileService(IDiskProvider diskProvider,
                                        IDiskScanService diskScanService,
                                        IEnumerable<IImportExistingExtraFiles> existingExtraFileImporters,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _existingExtraFileImporters = existingExtraFileImporters.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public void Handle(AuthorScannedEvent message)
        {
            var author = message.Author;
            var extraFiles = new List<ExtraFile>();

            // Audiobooks kept under a root of their own have their extras beside them there. A folder
            // that isn't on disk is left out, so its extras aren't taken to be gone.
            var authorFolders = new[] { author.Path, author.AudiobookPath }
                .Where(f => f.IsNotNullOrWhiteSpace() && _diskProvider.FolderExists(f))
                .Distinct(PathEqualityComparer.Instance)
                .ToList();

            if (!authorFolders.Any())
            {
                return;
            }

            var possibleExtraFiles = new List<string>();

            foreach (var authorFolder in authorFolders)
            {
                _logger.Debug("Looking for existing extra files in {0}", authorFolder);

                var filesOnDisk = _diskScanService.GetNonBookFiles(authorFolder);
                possibleExtraFiles.AddRange(_diskScanService.FilterPaths(authorFolder, filesOnDisk));
            }

            var filteredFiles = possibleExtraFiles.Distinct(PathEqualityComparer.Instance).ToList();
            var importedFiles = new List<string>();

            foreach (var existingExtraFileImporter in _existingExtraFileImporters)
            {
                var imported = existingExtraFileImporter.ProcessFiles(author, authorFolders, filteredFiles, importedFiles);

                importedFiles.AddRange(imported.Select(f => Path.Combine(author.Path, f.RelativePath)));
            }

            _logger.Info("Found {0} extra files", extraFiles.Count);
        }
    }
}
