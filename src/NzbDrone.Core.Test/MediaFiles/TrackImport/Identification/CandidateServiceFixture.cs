using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.BookImport;
using NzbDrone.Core.MediaFiles.BookImport.Identification;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.Goodreads;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Identification
{
    [TestFixture]
    public class CandidateServiceFixture : CoreTest<CandidateService>
    {
        [Test]
        public void should_not_throw_on_goodreads_exception()
        {
            Mocker.GetMock<ISearchForNewBook>()
                .Setup(s => s.SearchForNewBook(It.IsAny<string>(), It.IsAny<string>(), true))
                .Throws(new GoodreadsException("Bad search"));

            var edition = new LocalEdition
            {
                LocalBooks = new List<LocalBook>
                {
                    new LocalBook
                    {
                        FileTrackInfo = new ParsedTrackInfo
                        {
                            Authors = new List<string> { "Author" },
                            BookTitle = "Book"
                        }
                    }
                }
            };

            Subject.GetRemoteCandidates(edition, null).Should().BeEmpty();
        }

        [Test]
        public void should_only_count_existing_files_of_the_releases_own_format()
        {
            var book = Builder<Book>.CreateNew().With(b => b.Id = 1).Build();
            var edition = Builder<Edition>.CreateNew()
                .With(e => e.BookId = book.Id)
                .With(e => e.Ratings = new Ratings())
                .Build();

            Mocker.GetMock<IEditionService>()
                .Setup(s => s.GetEditionsByBook(book.Id))
                .Returns(new List<Edition> { edition });

            // The book already holds a multi-file audiobook.
            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesByBook(book.Id))
                .Returns(new List<BookFile>
                {
                    new BookFile { Path = "/audiobooks/Author/Book/01.mp3" },
                    new BookFile { Path = "/audiobooks/Author/Book/02.mp3" }
                });

            var localEdition = new LocalEdition
            {
                LocalBooks = new List<LocalBook>
                {
                    new LocalBook { Path = "/books/Author/Book/Book.epub", FileTrackInfo = new ParsedTrackInfo() }
                }
            };

            var candidates = Subject.GetDbCandidatesFromTags(localEdition, new IdentificationOverrides { Book = book }, true);

            candidates.Should().ContainSingle();
            candidates.Single().ExistingFiles.Should().BeEmpty();
        }
    }
}
