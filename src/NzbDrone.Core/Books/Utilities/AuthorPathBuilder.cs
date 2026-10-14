using System;
using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Books
{
    public interface IBuildAuthorPaths
    {
        string BuildPath(Author author, bool useExistingRelativeFolder);
        string BuildAudiobookPath(Author author, string existingAudiobookPath = null);
    }

    public class AuthorPathBuilder : IBuildAuthorPaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;

        public AuthorPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
        }

        public string BuildPath(Author author, bool useExistingRelativeFolder)
        {
            if (author.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(author));
            }

            if (useExistingRelativeFolder && author.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(author);
                return Path.Combine(author.RootFolderPath, relativePath);
            }

            return Path.Combine(author.RootFolderPath, _fileNameBuilder.GetAuthorFolder(author));
        }

        // Uses the same author folder naming as the main path, so audiobooks are laid out under
        // their root exactly the way ebooks are under theirs. A folder already in use keeps its
        // name - under the new root, if that changed - the way the main path does. Audiobooks are
        // never moved, so renaming the author or changing the folder format would otherwise send
        // new audiobooks to a second folder beside the first.
        public string BuildAudiobookPath(Author author, string existingAudiobookPath = null)
        {
            if (author.AudiobookRootFolderPath.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (existingAudiobookPath.IsNotNullOrWhiteSpace())
            {
                var existingRoot = _rootFolderService.GetBestRootFolderPath(existingAudiobookPath);

                if (existingRoot.IsNotNullOrWhiteSpace() && existingRoot.IsParentPath(existingAudiobookPath))
                {
                    return Path.Combine(author.AudiobookRootFolderPath, existingRoot.GetRelativePath(existingAudiobookPath));
                }
            }

            return Path.Combine(author.AudiobookRootFolderPath, _fileNameBuilder.GetAuthorFolder(author));
        }

        private string GetExistingRelativePath(Author author)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(author.Path);

            return rootFolderPath.GetRelativePath(author.Path);
        }
    }
}
