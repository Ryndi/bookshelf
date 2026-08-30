using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Books
{
    public static class BookFormat
    {
        private static readonly List<string> EbookFormats = new List<string> { "Kindle Edition", "Nook", "ebook" };

        private static readonly List<string> AudiobookFormats = new List<string> { "Audiobook", "Audio CD", "Audio Cassette", "Audible Audio", "CD-ROM", "MP3 CD" };

        public static bool IsAudiobook(string format)
        {
            return format.IsNotNullOrWhiteSpace() && AudiobookFormats.Contains(format);
        }

        public static bool IsEbook(string format)
        {
            return format.IsNotNullOrWhiteSpace() && EbookFormats.Contains(format);
        }

        // A book holds at most one monitored edition per format, so editions of unknown format are
        // grouped with ebooks - the same way an unrecognised file extension is treated as text.
        public static bool IsAudiobook(Edition edition)
        {
            return edition != null && IsAudiobook(edition.Format);
        }

        // Used wherever a single edition has to stand in for the book, such as the overview shown
        // in the UI. Ebook first, then lowest id, so the choice is stable between calls.
        public static Edition PrimaryMonitored(this IEnumerable<Edition> editions)
        {
            return editions.Where(x => x.Monitored)
                .OrderBy(x => IsAudiobook(x))
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }
    }
}
