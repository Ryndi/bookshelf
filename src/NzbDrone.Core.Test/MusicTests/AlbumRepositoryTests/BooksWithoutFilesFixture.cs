using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.BookRepositoryTests
{
    [TestFixture]
    public class BooksWithoutFilesFixture : DbTest<BookRepository, Book>
    {
        private Book _book;
        private Edition _edition;
        private int _fileCount;

        private void GivenBook(bool authorWantsAudiobooks, bool? bookWantsAudiobooks)
        {
            var meta = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);

            var author = Builder<Author>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.AuthorMetadataId = meta.Id)
                .With(a => a.Monitored = true)
                .With(a => a.SearchAudiobooks = authorWantsAudiobooks)
                .Build();
            Db.Insert(author);

            _book = Builder<Book>.CreateNew()
                .With(b => b.Id = 0)
                .With(b => b.AuthorMetadataId = meta.Id)
                .With(b => b.Monitored = true)
                .With(b => b.SearchAudiobooks = bookWantsAudiobooks)
                .With(b => b.ReleaseDate = DateTime.UtcNow.AddDays(-30))
                .Build();
            Db.Insert(_book);

            _edition = Builder<Edition>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.BookId = _book.Id)
                .With(e => e.Monitored = true)
                .With(e => e.ForeignEditionId = "edition1")
                .With(e => e.TitleSlug = "slug1")
                .Build();
            Db.Insert(_edition);
        }

        private void GivenFile(Quality quality)
        {
            _fileCount++;

            var file = Builder<BookFile>.CreateNew()
                .With(f => f.Id = 0)
                .With(f => f.EditionId = _edition.Id)
                .With(f => f.Path = $"/books/file{_fileCount}")
                .With(f => f.Quality = new QualityModel(quality))
                .Build();

            Db.Insert(file);
        }

        private List<Book> Missing()
        {
            var spec = new PagingSpec<Book>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "Id",
                SortDirection = SortDirection.Ascending
            };

            return Subject.BooksWithoutFiles(spec).Records.ToList();
        }

        [Test]
        public void should_want_a_book_with_no_files_at_all()
        {
            GivenBook(false, null);

            Missing().Should().HaveCount(1);
        }

        [Test]
        public void should_not_want_a_book_that_has_an_ebook_when_audiobooks_are_off()
        {
            GivenBook(false, null);
            GivenFile(Quality.EPUB);

            Missing().Should().BeEmpty();
        }

        [Test]
        public void should_still_want_the_audiobook_when_only_the_ebook_is_on_disk()
        {
            GivenBook(true, null);
            GivenFile(Quality.EPUB);

            Missing().Should().HaveCount(1);
        }

        [Test]
        public void should_still_want_the_ebook_when_only_the_audiobook_is_on_disk()
        {
            GivenBook(true, null);
            GivenFile(Quality.M4B);

            Missing().Should().HaveCount(1);
        }

        [Test]
        public void should_not_want_a_book_that_has_both_formats()
        {
            GivenBook(true, null);
            GivenFile(Quality.EPUB);
            GivenFile(Quality.M4B);

            Missing().Should().BeEmpty();
        }

        [Test]
        public void book_setting_should_override_the_author_when_switched_on()
        {
            GivenBook(false, true);
            GivenFile(Quality.EPUB);

            Missing().Should().HaveCount(1);
        }

        [Test]
        public void book_setting_should_override_the_author_when_switched_off()
        {
            GivenBook(true, false);
            GivenFile(Quality.EPUB);

            Missing().Should().BeEmpty();
        }
    }
}
