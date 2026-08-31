using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.MediaFiles.BookImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        BookFileMoveResult UpgradeBookFile(BookFile bookFile, LocalBook localBook, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMetadataTagService _metadataTagService;
        private readonly IMoveBookFiles _bookFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly ICalibreProxy _calibre;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMetadataTagService metadataTagService,
                                       IMoveBookFiles bookFileMover,
                                       IDiskProvider diskProvider,
                                       IRootFolderService rootFolderService,
                                       ICalibreProxy calibre,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _metadataTagService = metadataTagService;
            _bookFileMover = bookFileMover;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
            _calibre = calibre;
            _logger = logger;
        }

        public BookFileMoveResult UpgradeBookFile(BookFile bookFile, LocalBook localBook, bool copyOnly = false)
        {
            var moveFileResult = new BookFileMoveResult();
            var allFiles = localBook.Book.BookFiles.Value;

            // An ebook and an audiobook of the same book can be held side by side, so only files of the
            // same type are replaced by the incoming one.
            var isAudio = MediaFileExtensions.IsAudioFile(bookFile.Path);
            var existingFiles = allFiles.Where(f => MediaFileExtensions.IsAudioFile(f.Path) == isAudio).ToList();

            var rootFolderPath = _diskProvider.GetParentFolder(localBook.Author.Path);
            var rootFolder = _rootFolderService.GetBestRootFolder(rootFolderPath);
            var isCalibre = rootFolder.IsCalibreLibrary && rootFolder.CalibreSettings != null;

            var settings = rootFolder.CalibreSettings;

            // Calibre keeps every format under a single book, so keep the link even when the only
            // files we have for this book are of the other type.
            var existingCalibreId = allFiles.FirstOrDefault(f => f.CalibreId > 0)?.CalibreId ?? 0;

            if (existingCalibreId > 0)
            {
                bookFile.CalibreId = existingCalibreId;
            }

            // If there are existing book files and the root folder is missing, throw, so the old file isn't left behind during the import process.
            if (existingFiles.Any() && !_diskProvider.FolderExists(rootFolderPath))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolderPath}' was not found.");
            }

            foreach (var file in existingFiles)
            {
                var bookFilePath = file.Path;
                var subfolder = rootFolderPath.GetRelativePath(_diskProvider.GetParentFolder(bookFilePath));

                if (_diskProvider.FileExists(bookFilePath))
                {
                    _logger.Debug("Removing existing book file: {0} CalibreId: {1}", file, file.CalibreId);

                    if (!isCalibre)
                    {
                        _recycleBinProvider.DeleteFile(bookFilePath, subfolder);
                    }
                    else
                    {
                        var existing = _calibre.GetBook(file.CalibreId, settings);
                        var existingFormats = existing.Formats.Keys.Where(f => MediaFileExtensions.AudioExtensions.Contains("." + f) == isAudio).ToList();
                        _logger.Debug($"Removing existing formats {existingFormats.ConcatToString()} from calibre");
                        _calibre.RemoveFormats(file.CalibreId, existingFormats, settings);
                    }
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            if (!isCalibre)
            {
                if (copyOnly)
                {
                    moveFileResult.BookFile = _bookFileMover.CopyBookFile(bookFile, localBook);
                }
                else
                {
                    moveFileResult.BookFile = _bookFileMover.MoveBookFile(bookFile, localBook);
                }

                _metadataTagService.WriteTags(bookFile, true);
            }
            else
            {
                var source = bookFile.Path;

                moveFileResult.BookFile = _calibre.AddAndConvert(bookFile, settings);

                if (!copyOnly)
                {
                    _diskProvider.DeleteFile(source);
                }
            }

            return moveFileResult;
        }
    }
}
