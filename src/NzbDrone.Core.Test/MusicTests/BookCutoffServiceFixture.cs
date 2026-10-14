using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class BookCutoffServiceFixture : DbTest<BookCutoffService, Book>
    {
        // Laid out the way the default profiles are, with every audio quality after the ebook ones.
        private static readonly Quality[] Order =
        {
            Quality.Unknown, Quality.PDF, Quality.MOBI, Quality.EPUB, Quality.AZW3,
            Quality.UnknownAudio, Quality.MP3, Quality.M4B, Quality.FLAC
        };

        private Edition _edition;

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IBookRepository>(Mocker.Resolve<BookRepository>());

            // The author's main profile takes no audio at all; audiobooks are judged by a profile of
            // their own, which upgrades them to M4B.
            var ebookProfile = GivenProfile(1, Quality.AZW3, Quality.EPUB, Quality.AZW3);
            var audiobookProfile = GivenProfile(2, Quality.M4B, Quality.UnknownAudio, Quality.MP3, Quality.M4B, Quality.FLAC);

            Mocker.GetMock<IQualityProfileService>()
                .Setup(s => s.All())
                .Returns(new List<QualityProfile> { ebookProfile, audiobookProfile });

            var meta = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);

            var author = Builder<Author>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.AuthorMetadataId = meta.Id)
                .With(a => a.QualityProfileId = ebookProfile.Id)
                .With(a => a.AudiobookQualityProfileId = audiobookProfile.Id)
                .Build();
            Db.Insert(author);

            var book = Builder<Book>.CreateNew()
                .With(b => b.Id = 0)
                .With(b => b.AuthorMetadataId = meta.Id)
                .Build();
            Db.Insert(book);

            _edition = Builder<Edition>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.BookId = book.Id)
                .With(e => e.Monitored = true)
                .Build();
            Db.Insert(_edition);
        }

        private static QualityProfile GivenProfile(int id, Quality cutoff, params Quality[] allowed)
        {
            return new QualityProfile
            {
                Id = id,
                Name = $"Profile {id}",
                UpgradeAllowed = true,
                Cutoff = cutoff.Id,
                Items = Order.Select(q => new QualityProfileQualityItem { Quality = q, Allowed = allowed.Contains(q) }).ToList(),
                FormatItems = new List<ProfileFormatItem>()
            };
        }

        private void GivenFile(Quality quality)
        {
            var file = Builder<BookFile>.CreateNew()
                .With(f => f.Id = 0)
                .With(f => f.EditionId = _edition.Id)
                .With(f => f.Path = "/books/file")
                .With(f => f.Quality = new QualityModel(quality))
                .Build();

            Db.Insert(file);
        }

        private List<Book> CutoffUnmet()
        {
            var spec = new PagingSpec<Book>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "Id",
                SortDirection = SortDirection.Ascending
            };

            return Subject.BooksWhereCutoffUnmet(spec).Records.ToList();
        }

        [Test]
        public void should_list_an_audiobook_below_the_cutoff_of_the_audiobook_profile()
        {
            // Judged by the main profile instead, which has no audio cutoff, it would never be listed.
            GivenFile(Quality.MP3);

            CutoffUnmet().Should().HaveCount(1);
        }

        [Test]
        public void should_not_list_an_audiobook_that_meets_the_cutoff_of_the_audiobook_profile()
        {
            GivenFile(Quality.M4B);

            CutoffUnmet().Should().BeEmpty();
        }

        [Test]
        public void should_list_an_ebook_below_the_cutoff_of_the_main_profile()
        {
            GivenFile(Quality.EPUB);

            CutoffUnmet().Should().HaveCount(1);
        }
    }
}
