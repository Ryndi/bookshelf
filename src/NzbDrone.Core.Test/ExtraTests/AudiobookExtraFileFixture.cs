using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ExtraTests
{
    // Extras imported beside an audiobook kept under a root of its own, outside the author's folder.
    [TestFixture]
    public class AudiobookExtraFileFixture : CoreTest<OtherExtraService>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Books\Author".AsOsAgnostic())
                .With(a => a.AudiobookPath = @"D:\Audiobooks\Author".AsOsAgnostic())
                .Build();
        }

        private BookFile GivenBookFile(string path)
        {
            return Builder<BookFile>.CreateNew()
                .With(f => f.Path = path)
                .With(f => f.Edition = new Edition { BookId = 1 })
                .Build();
        }

        [Test]
        public void should_store_an_extra_beside_an_ebook_relative_to_the_author_folder()
        {
            var ebook = GivenBookFile(@"C:\Books\Author\Book\Book.epub".AsOsAgnostic());

            var extraFile = Subject.Import(_author, ebook, @"C:\Downloads\Book\Book.jpg".AsOsAgnostic(), ".jpg", false);

            extraFile.RelativePath.Should().Be(Path.Combine("Book", "Book.jpg"));
        }

        [Test]
        public void should_store_an_extra_beside_an_audiobook_under_its_own_root_by_its_full_path()
        {
            var audiobook = GivenBookFile(@"D:\Audiobooks\Author\Book\Book.m4b".AsOsAgnostic());
            var expectedPath = @"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic();

            var extraFile = Subject.Import(_author, audiobook, @"C:\Downloads\Book\Book.jpg".AsOsAgnostic(), ".jpg", false);

            extraFile.RelativePath.Should().Be(expectedPath);

            // Read back the way every reader of extras does, it comes out as the file itself.
            Path.Combine(_author.Path, extraFile.RelativePath).Should().Be(expectedPath);

            Mocker.GetMock<IOtherExtraFileService>()
                  .Verify(v => v.Upsert(It.Is<OtherExtraFile>(e => e.RelativePath == expectedPath)), Times.Once());
        }

        [Test]
        public void should_keep_an_extra_beside_an_audiobook_by_its_full_path_after_a_rename()
        {
            var audiobook = GivenBookFile(@"D:\Audiobooks\Author\Book\Renamed.m4b".AsOsAgnostic());
            audiobook.Id = 5;

            var extraFile = new OtherExtraFile
            {
                BookFileId = audiobook.Id,
                RelativePath = @"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic(),
                Extension = ".jpg"
            };

            Mocker.GetMock<IOtherExtraFileService>()
                  .Setup(s => s.GetFilesByAuthor(_author.Id))
                  .Returns(new List<OtherExtraFile> { extraFile });

            Subject.MoveFilesAfterRename(_author, new List<BookFile> { audiobook });

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(@"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic(), @"D:\Audiobooks\Author\Book\Renamed.jpg".AsOsAgnostic(), false), Times.Once());

            extraFile.RelativePath.Should().Be(@"D:\Audiobooks\Author\Book\Renamed.jpg".AsOsAgnostic());
        }
    }

    [TestFixture]
    public class AudiobookExtraFileDeletionFixture : CoreTest<OtherExtraFileService>
    {
        [Test]
        public void should_recycle_an_extra_beside_an_audiobook_under_its_own_root()
        {
            var author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Books\Author".AsOsAgnostic())
                .With(a => a.AudiobookPath = @"D:\Audiobooks\Author".AsOsAgnostic())
                .Build();

            var audiobook = Builder<BookFile>.CreateNew()
                .With(f => f.Id = 5)
                .With(f => f.Path = @"D:\Audiobooks\Author\Book\Book.m4b".AsOsAgnostic())
                .With(f => f.Author = author)
                .Build();

            var extraPath = @"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic();

            Mocker.GetMock<IExtraFileRepository<OtherExtraFile>>()
                  .Setup(s => s.GetFilesByBookFile(audiobook.Id))
                  .Returns(new List<OtherExtraFile> { new OtherExtraFile { BookFileId = audiobook.Id, RelativePath = extraPath } });

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(extraPath))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns<string>(p => Path.GetDirectoryName(p));

            Subject.Handle(new BookFileDeletedEvent(audiobook, DeleteMediaFileReason.Manual));

            Mocker.GetMock<IRecycleBinProvider>()
                  .Verify(v => v.DeleteFile(extraPath, Path.Combine("Author", "Book")), Times.Once());
        }
    }

    [TestFixture]
    public class AudiobookExtraFileScanFixture : CoreTest<ExistingExtraFileService>
    {
        [Test]
        public void should_look_for_extras_in_the_audiobook_folder_too()
        {
            var author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Books\Author".AsOsAgnostic())
                .With(a => a.AudiobookPath = @"D:\Audiobooks\Author".AsOsAgnostic())
                .Build();

            var ebookExtra = @"C:\Books\Author\Book\Book.jpg".AsOsAgnostic();
            var audiobookExtra = @"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.GetNonBookFiles(author.Path, true))
                  .Returns(new[] { ebookExtra });

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.GetNonBookFiles(author.AudiobookPath, true))
                  .Returns(new[] { audiobookExtra });

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                  .Returns<string, IEnumerable<string>>((basePath, paths) => paths.ToList());

            var importer = new Mock<IImportExistingExtraFiles>();
            importer.Setup(s => s.ProcessFiles(It.IsAny<Author>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                    .Returns(new List<ExtraFile>());

            Mocker.SetConstant<IEnumerable<IImportExistingExtraFiles>>(new[] { importer.Object });

            Subject.Handle(new AuthorScannedEvent(author));

            importer.Verify(v => v.ProcessFiles(author,
                                                It.Is<List<string>>(f => f.SequenceEqual(new[] { author.Path, author.AudiobookPath })),
                                                It.Is<List<string>>(f => f.SequenceEqual(new[] { ebookExtra, audiobookExtra })),
                                                It.IsAny<List<string>>()),
                            Times.Once());
        }
    }

    [TestFixture]
    public class AudiobookExistingExtraFileFixture : CoreTest<ExistingOtherExtraImporter>
    {
        [Test]
        public void should_not_forget_an_extra_in_a_folder_that_was_not_scanned()
        {
            var author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Books\Author".AsOsAgnostic())
                .With(a => a.AudiobookPath = @"D:\Audiobooks\Author".AsOsAgnostic())
                .Build();

            // Neither is on disk, but only the main folder was looked through: the audiobook drive
            // isn't there just now.
            var ebookExtra = new OtherExtraFile { Id = 1, RelativePath = Path.Combine("Book", "Book.jpg") };
            var audiobookExtra = new OtherExtraFile { Id = 2, RelativePath = @"D:\Audiobooks\Author\Book\Book.jpg".AsOsAgnostic() };

            Mocker.GetMock<IExtraFileService<OtherExtraFile>>()
                  .Setup(s => s.GetFilesByAuthor(author.Id))
                  .Returns(new List<OtherExtraFile> { ebookExtra, audiobookExtra });

            Subject.ProcessFiles(author, new List<string> { author.Path }, new List<string>(), new List<string>());

            Mocker.GetMock<IExtraFileService<OtherExtraFile>>()
                  .Verify(v => v.DeleteMany(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1 }))), Times.Once());
        }
    }
}
