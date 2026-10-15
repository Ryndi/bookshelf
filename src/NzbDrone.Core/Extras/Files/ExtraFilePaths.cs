using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.Extras.Files
{
    public static class ExtraFilePaths
    {
        // Extras are stored relative to the author's folder. One kept beside an audiobook under a root
        // of its own isn't in that folder, so it is stored by its full path instead, the way book files
        // are. Joined to the author's folder with Path.Combine, a full path stays as it is, so reading
        // either kind back is the same.
        public static string GetExtraFileRelativePath(this Author author, string path)
        {
            return author.Path.IsParentPath(path) ? author.Path.GetRelativePath(path) : path;
        }

        // The folders above an extra in the recycle bin: from the author's folder holding it - the
        // audiobook folder, for one kept beside an audiobook under a root of its own - down.
        public static string GetRecycleBinSubfolder(this Author author, IDiskProvider diskProvider, string path)
        {
            var authorFolder = new[] { author.Path, author.AudiobookPath }
                .FirstOrDefault(f => f.IsNotNullOrWhiteSpace() && f.IsParentPath(path));

            if (authorFolder == null)
            {
                return string.Empty;
            }

            return diskProvider.GetParentFolder(authorFolder).GetRelativePath(diskProvider.GetParentFolder(path));
        }
    }
}
