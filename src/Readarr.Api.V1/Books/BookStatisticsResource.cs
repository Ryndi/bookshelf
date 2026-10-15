using NzbDrone.Core.AuthorStats;

namespace Readarr.Api.V1.Books
{
    public class BookStatisticsResource
    {
        public int BookFileCount { get; set; }
        public int BookCount { get; set; }
        public int TotalBookCount { get; set; }

        // Which formats the book actually holds, so a client can say what is still missing
        // rather than only that something is. EbookCount and AudiobookCount are zero unless the
        // book wants that format at all.
        public int EbookCount { get; set; }
        public int AvailableEbookCount { get; set; }
        public int AudiobookCount { get; set; }
        public int AvailableAudiobookCount { get; set; }
        public long SizeOnDisk { get; set; }

        public decimal PercentOfBooks
        {
            get
            {
                if (BookCount == 0)
                {
                    return 0;
                }

                return BookFileCount / (decimal)BookCount * 100;
            }
        }
    }

    public static class BookStatisticsResourceMapper
    {
        public static BookStatisticsResource ToResource(this BookStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new BookStatisticsResource
            {
                BookFileCount = model.BookFileCount,
                BookCount = model.BookCount,
                SizeOnDisk = model.SizeOnDisk,
                TotalBookCount = model.TotalBookCount,
                EbookCount = model.EbookCount,
                AvailableEbookCount = model.AvailableEbookCount,
                AudiobookCount = model.AudiobookCount,
                AvailableAudiobookCount = model.AvailableAudiobookCount
            };
        }
    }
}
