using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Books
{
    public static class BookFormat
    {
        private static readonly List<string> EbookFormats = new List<string> { "Kindle Edition", "Nook", "ebook" };

        private static readonly List<string> AudiobookFormats = new List<string> { "Audiobook", "Audio CD", "Audio Cassette", "Audible Audio", "CD-ROM", "MP3 CD" };

        // Metadata carries this as free text and it is inconsistent - "Paperback" and "paperback"
        // both occur, along with the odd typo - so matching is case insensitive, and a keyword
        // catches the wordings the fixed lists miss, such as "Digital Audiobook".
        private static readonly List<string> AudiobookKeywords = new List<string> { "audio", "audible", "cassette", "spoken" };

        private static readonly List<string> EbookKeywords = new List<string> { "ebook", "e-book", "epub", "kindle", "nook", "mobi", "azw" };

        public static bool IsAudiobook(string format)
        {
            return Matches(format, AudiobookFormats, AudiobookKeywords);
        }

        public static bool IsEbook(string format)
        {
            return Matches(format, EbookFormats, EbookKeywords);
        }

        private static bool Matches(string format, List<string> known, List<string> keywords)
        {
            if (format.IsNullOrWhiteSpace())
            {
                return false;
            }

            var trimmed = format.Trim();

            return known.Any(x => x.Equals(trimmed, StringComparison.InvariantCultureIgnoreCase)) ||
                   keywords.Any(x => trimmed.Contains(x, StringComparison.InvariantCultureIgnoreCase));
        }

        // A book holds at most one monitored edition per format, so editions of unknown format are
        // grouped with ebooks - the same way an unrecognised file extension is treated as text.
        public static bool IsAudiobook(Edition edition)
        {
            return edition != null && IsAudiobook(edition.Format);
        }

        // Picks the monitored edition matching a file's own format, so an audiobook is not filed
        // against the ebook edition. Falls back to the primary when that format is not monitored.
        public static Edition MonitoredForFormat(this IEnumerable<Edition> editions, bool audio)
        {
            var all = editions as IList<Edition> ?? editions?.ToList();

            if (all == null)
            {
                return null;
            }

            return all.FirstOrDefault(x => x.Monitored && IsAudiobook(x) == audio) ?? all.PrimaryMonitored();
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
