using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AudiobookWantedSpecificationFixture : CoreTest<AudiobookWantedSpecification>
    {
        private Author _author;
        private Book _book;
        private RemoteBook _remoteBook;

        [SetUp]
        public void Setup()
        {
            // A profile that takes either format, so nothing but the setting can turn an
            // audiobook away.
            _author = Builder<Author>.CreateNew()
                .With(a => a.SearchAudiobooks = false)
                .With(a => a.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                .Build();

            _book = Builder<Book>.CreateNew()
                .With(b => b.SearchAudiobooks = null)
                .Build();

            _remoteBook = new RemoteBook
            {
                Author = _author,
                Books = new List<Book> { _book },
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.M4B) }
            };
        }

        [Test]
        public void should_accept_an_ebook_whatever_the_setting()
        {
            _remoteBook.ParsedBookInfo.Quality = new QualityModel(Quality.EPUB);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_an_audiobook_when_audiobooks_are_off()
        {
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_an_audiobook_when_the_author_wants_them()
        {
            _author.SearchAudiobooks = true;

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_an_audiobook_when_the_book_wants_one()
        {
            _book.SearchAudiobooks = true;

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_an_audiobook_when_the_book_turns_them_off()
        {
            _author.SearchAudiobooks = true;
            _book.SearchAudiobooks = false;

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_an_audiobook_for_an_author_whose_profile_takes_only_audio()
        {
            // An audiobook library from before audiobooks had a setting of their own.
            _author.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3, Quality.M4B) };

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }
    }
}
