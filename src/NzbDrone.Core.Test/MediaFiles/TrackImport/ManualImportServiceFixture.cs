using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.BookImport;
using NzbDrone.Core.MediaFiles.BookImport.Manual;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.BookImport
{
    [TestFixture]
    public class ManualImportServiceFixture : FileSystemTest<ManualImportService>
    {
        private Author _author;
        private Book _book;
        private Edition _ebookEdition;
        private Edition _audiobookEdition;
        private List<ImportDecision<LocalBook>> _imported;

        [SetUp]
        public void Setup()
        {
            _imported = new List<ImportDecision<LocalBook>>();

            _author = Builder<Author>.CreateNew()
                .With(a => a.Id = 1)
                .With(a => a.Path = @"C:\Books\Some Author".AsOsAgnostic())
                .Build();

            _ebookEdition = Builder<Edition>.CreateNew()
                .With(e => e.Id = 1)
                .With(e => e.ForeignEditionId = "ebook")
                .With(e => e.Format = "Kindle Edition")
                .With(e => e.Monitored = true)
                .Build();

            _audiobookEdition = Builder<Edition>.CreateNew()
                .With(e => e.Id = 2)
                .With(e => e.ForeignEditionId = "audiobook")
                .With(e => e.Format = "Audiobook")
                .With(e => e.Monitored = true)
                .Build();

            _book = Builder<Book>.CreateNew()
                .With(b => b.Id = 1)
                .Build();

            _book.Editions = new List<Edition> { _ebookEdition, _audiobookEdition };

            Mocker.GetMock<IAuthorService>()
                .Setup(s => s.GetAuthor(_author.Id))
                .Returns(_author);

            Mocker.GetMock<IBookService>()
                .Setup(s => s.GetBook(_book.Id))
                .Returns(_book);

            GivenMetadataFor(_book);

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolder(_author.Path))
                .Returns(new RootFolder { Path = @"C:\Books".AsOsAgnostic() });

            Mocker.GetMock<IImportApprovedBooks>()
                .Setup(s => s.Import(It.IsAny<List<ImportDecision<LocalBook>>>(), It.IsAny<bool>(), It.IsAny<DownloadClientItem>(), It.IsAny<ImportMode>()))
                .Callback<List<ImportDecision<LocalBook>>, bool, DownloadClientItem, ImportMode>((decisions, replace, item, mode) => _imported.AddRange(decisions))
                .Returns(new List<ImportResult>());
        }

        private void GivenMetadataFor(Book book)
        {
            Mocker.GetMock<IProvideBookInfo>()
                .Setup(s => s.GetBookInfo(It.IsAny<string>()))
                .Returns(Tuple.Create("author", book, new List<AuthorMetadata>()));
        }

        private void GivenEditionInLibrary(Edition edition)
        {
            Mocker.GetMock<IEditionService>()
                .Setup(s => s.GetEditionByForeignEditionId(edition.ForeignEditionId))
                .Returns(edition);
        }

        private ManualImportFile GivenFile(string name, Quality quality, string foreignEditionId = null)
        {
            var path = Path.Combine(@"C:\Downloads".AsOsAgnostic(), name);

            FileSystem.AddFile(path, new MockFileData("book"));

            return new ManualImportFile
            {
                Path = path,
                AuthorId = _author.Id,
                BookId = _book.Id,
                ForeignEditionId = foreignEditionId,
                Quality = new QualityModel(quality)
            };
        }

        private void Execute(params ManualImportFile[] files)
        {
            Subject.Execute(new ManualImportCommand
            {
                Files = files.ToList(),
                ImportMode = ImportMode.Auto
            });
        }

        [Test]
        public void should_import_an_audiobook_against_the_monitored_audiobook_edition_when_none_is_named()
        {
            // Nothing requires a command to name an edition for each file.
            Execute(GivenFile("Book.m4b", Quality.M4B));

            _imported.Should().ContainSingle();
            _imported.Single().Item.Edition.Should().Be(_audiobookEdition);
        }

        [Test]
        public void should_import_an_ebook_against_the_monitored_ebook_edition_when_none_is_named()
        {
            Execute(GivenFile("Book.epub", Quality.EPUB));

            _imported.Single().Item.Edition.Should().Be(_ebookEdition);
        }

        [Test]
        public void should_import_against_the_edition_that_was_named()
        {
            var chosen = Builder<Edition>.CreateNew()
                .With(e => e.Id = 3)
                .With(e => e.ForeignEditionId = "chosen")
                .With(e => e.Format = "ebook")
                .With(e => e.Monitored = false)
                .Build();

            GivenEditionInLibrary(chosen);

            Execute(GivenFile("Book.epub", Quality.EPUB, chosen.ForeignEditionId));

            _imported.Single().Item.Edition.Should().Be(chosen);
        }

        [Test]
        public void should_fetch_a_named_edition_the_library_does_not_have_from_the_metadata_source()
        {
            var remote = Builder<Edition>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.ForeignEditionId = "remote")
                .Build();

            var remoteBook = Builder<Book>.CreateNew().Build();
            remoteBook.Editions = new List<Edition> { remote };

            GivenMetadataFor(remoteBook);

            Execute(GivenFile("Book.epub", Quality.EPUB, remote.ForeignEditionId));

            _imported.Single().Item.Edition.Should().Be(remote);
        }

        [Test]
        public void should_fall_back_to_the_monitored_edition_when_the_named_one_cannot_be_found()
        {
            // Neither the library nor the metadata source knows it.
            Execute(GivenFile("Book.m4b", Quality.M4B, "missing"));

            _imported.Single().Item.Edition.Should().Be(_audiobookEdition);
        }

        [Test]
        public void should_not_ask_the_metadata_source_when_no_edition_is_named()
        {
            // No edition there could match an empty id, so the round trip can never help.
            Execute(GivenFile("Book.epub", Quality.EPUB));

            Mocker.GetMock<IProvideBookInfo>()
                .Verify(v => v.GetBookInfo(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_turn_a_file_away_when_the_book_has_no_edition_to_import_against()
        {
            _book.Editions = new List<Edition>();

            Execute(GivenFile("Book.epub", Quality.EPUB));

            _imported.Single().Approved.Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
