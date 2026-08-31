using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AuthorStats
{
    public class BookStatistics : ResultSet
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int BookFileCount { get; set; }
        public int BookCount { get; set; }
        public int AvailableBookCount { get; set; }
        public int TotalBookCount { get; set; }

        // Counted separately from AvailableBookCount, which is satisfied by a file of either
        // format. AudiobookCount only counts books that actually want an audiobook, so it is
        // the denominator for audiobook progress rather than the book count.
        public int AvailableEbookCount { get; set; }
        public int AudiobookCount { get; set; }
        public int AvailableAudiobookCount { get; set; }
        public long SizeOnDisk { get; set; }
    }
}
