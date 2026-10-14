using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, EntityHistory>
    {
        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<EntityHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void should_get_download_history()
        {
            var historyBluray = Builder<EntityHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.MP3))
                .With(c => c.AuthorId = 12)
                .With(c => c.EventType = EntityHistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<EntityHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.AZW3))
                .With(c => c.AuthorId = 12)
                .With(c => c.EventType = EntityHistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyBluray);
            Subject.Insert(historyDvd);

            var downloadHistory = Subject.FindDownloadHistory(12, new QualityModel(Quality.MP3));

            downloadHistory.Should().HaveCount(1);
        }

        [Test]
        public void should_find_the_most_recent_event_for_a_book_in_one_format()
        {
            var audiobookGrab = Builder<EntityHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.M4B))
                .With(c => c.BookId = 5)
                .With(c => c.Date = DateTime.UtcNow.AddHours(-2))
                .BuildNew();

            var ebookGrab = Builder<EntityHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.EPUB))
                .With(c => c.BookId = 5)
                .With(c => c.Date = DateTime.UtcNow.AddHours(-1))
                .BuildNew();

            Subject.Insert(audiobookGrab);
            Subject.Insert(ebookGrab);

            Subject.MostRecentForBook(5, true).Quality.Quality.Should().Be(Quality.M4B);
            Subject.MostRecentForBook(5, false).Quality.Quality.Should().Be(Quality.EPUB);
        }
    }
}
