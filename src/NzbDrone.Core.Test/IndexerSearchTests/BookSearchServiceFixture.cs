using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class BookSearchServiceFixture : CoreTest
    {
        private Author _author;
        private Book _book;
        private List<Core.Queue.Queue> _queue;

        [SetUp]
        public void Setup()
        {
            var ebookProfile = new QualityProfile { Id = 1, Items = Qualities.QualityFixture.GetDefaultQualities(Quality.EPUB, Quality.AZW3) };
            var audiobookOnlyProfile = new QualityProfile { Id = 2, Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3, Quality.M4B) };

            _author = Builder<Author>.CreateNew()
                .With(a => a.QualityProfileId = ebookProfile.Id)
                .With(a => a.SearchAudiobooks = true)
                .Build();

            _book = Builder<Book>.CreateNew()
                .With(b => b.Id = 1)
                .With(b => b.SearchAudiobooks = null)
                .With(b => b.Author = _author)
                .Build();

            _queue = new List<Core.Queue.Queue>();

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(_queue);

            Mocker.GetMock<IQualityProfileService>()
                .Setup(s => s.All())
                .Returns(new List<QualityProfile> { ebookProfile, audiobookOnlyProfile });
        }

        private void GivenQueued(Quality quality)
        {
            _queue.Add(new Core.Queue.Queue { Book = _book, Quality = new QualityModel(quality) });
        }

        // The service is internal, so it cannot be the subject of a public fixture.
        private List<Book> Searched()
        {
            return Mocker.Resolve<BookSearchService>().NotFullyQueued(new List<Book> { _book });
        }

        [Test]
        public void should_search_a_book_with_nothing_queued()
        {
            Searched().Should().ContainSingle();
        }

        [Test]
        public void should_search_for_the_audiobook_while_the_ebook_downloads()
        {
            GivenQueued(Quality.EPUB);

            Searched().Should().ContainSingle();
        }

        [Test]
        public void should_not_search_while_both_formats_download()
        {
            GivenQueued(Quality.EPUB);
            GivenQueued(Quality.M4B);

            Searched().Should().BeEmpty();
        }

        [Test]
        public void should_not_search_a_book_wanting_only_an_ebook_while_it_downloads()
        {
            _author.SearchAudiobooks = false;
            GivenQueued(Quality.EPUB);

            Searched().Should().BeEmpty();
        }

        [Test]
        public void should_not_search_an_audiobook_library_while_its_audiobook_downloads()
        {
            _author.SearchAudiobooks = false;
            _author.QualityProfileId = 2;
            GivenQueued(Quality.M4B);

            Searched().Should().BeEmpty();
        }
    }
}
