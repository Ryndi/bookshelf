using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    // Audiobooks usually sit under a different root to the ebooks, and that folder would
    // otherwise only appear on the first successful import. Creating it as soon as the setting
    // is turned on makes the destination visible, and gives somewhere to drop files for a
    // manual import.
    public class AudiobookFolderHandler : IHandle<AuthorAddedEvent>, IHandle<AuthorEditedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderWatchingService _rootFolderWatchingService;
        private readonly Logger _logger;

        public AudiobookFolderHandler(IDiskProvider diskProvider,
                                      IRootFolderWatchingService rootFolderWatchingService,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _rootFolderWatchingService = rootFolderWatchingService;
            _logger = logger;
        }

        public void Handle(AuthorAddedEvent message)
        {
            EnsureAudiobookFolder(message.Author);
        }

        public void Handle(AuthorEditedEvent message)
        {
            EnsureAudiobookFolder(message.Author);
        }

        private void EnsureAudiobookFolder(Author author)
        {
            if (author == null || !author.SearchAudiobooks || author.AudiobookPath.IsNullOrWhiteSpace())
            {
                return;
            }

            if (_diskProvider.FolderExists(author.AudiobookPath))
            {
                return;
            }

            var root = Path.GetDirectoryName(author.AudiobookPath);

            // The root itself is left alone. If it is missing the volume is most likely not
            // mounted, and creating it would write to the wrong place and hide that.
            if (!_diskProvider.FolderExists(root))
            {
                _logger.Warn("Audiobook root folder '{0}' was not found, not creating '{1}'", root, author.AudiobookPath);
                return;
            }

            _logger.Info("Creating audiobook folder '{0}' for {1}", author.AudiobookPath, author);

            _rootFolderWatchingService.ReportFileSystemChangeBeginning(author.AudiobookPath);
            _diskProvider.CreateFolder(author.AudiobookPath);
        }
    }
}
