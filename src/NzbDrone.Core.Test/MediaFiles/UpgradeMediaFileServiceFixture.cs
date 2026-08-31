using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class UpgradeMediaFileServiceFixture : CoreTest<UpgradeMediaFileService>
    {
        private BookFile _trackFile;
        private LocalBook _localTrack;
        private string _rootPath = @"C:\Test\Music\Author".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            _localTrack = new LocalBook();
            _localTrack.Author = new Author
            {
                Path = _rootPath
            };

            _trackFile = Builder<BookFile>
                .CreateNew()
                .Build();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                 .Setup(c => c.GetParentFolder(It.IsAny<string>()))
                 .Returns<string>(c => Path.GetDirectoryName(c));

            Mocker.GetMock<IRootFolderService>()
                .Setup(c => c.GetBestRootFolder(It.IsAny<string>()))
                .Returns(new RootFolder());
        }

        private void GivenSingleTrackWithSingleTrackFile()
        {
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(
                          new List<BookFile>
                          {
                              new BookFile
                              {
                                  Id = 1,
                                  Path = Path.Combine(_rootPath, @"Season 01\30.rock.s01e01.avi"),
                              }
                          }))
                .Build();
        }

        private void GivenBookWithFiles(params string[] fileNames)
        {
            var files = fileNames.Select((name, index) => new BookFile
            {
                Id = index + 1,
                Path = Path.Combine(_rootPath, name)
            }).ToList();

            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(files))
                .Build();
        }

        [Test]
        public void should_delete_single_track_file_once()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_track_file_from_database()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<BookFile>(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_delete_existing_file_fromdb_if_file_doesnt_exist()
        {
            GivenSingleTrackWithSingleTrackFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            // Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localTrack.Book.BookFiles.Value, DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_not_try_to_recyclebin_existing_file_if_file_doesnt_exist()
        {
            GivenSingleTrackWithSingleTrackFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_old_track_file_in_oldFiles()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_audiobook_when_importing_ebook()
        {
            GivenBookWithFiles("book.m4b");
            _trackFile.Path = Path.Combine(_rootPath, "book.epub");

            Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles.Should().BeEmpty();

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<BookFile>(), It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }

        [Test]
        public void should_not_delete_ebook_when_importing_audiobook()
        {
            GivenBookWithFiles("book.epub");
            _trackFile.Path = Path.Combine(_rootPath, "book.m4b");

            Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles.Should().BeEmpty();

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<BookFile>(), It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }

        [Test]
        public void should_only_replace_existing_file_of_the_same_type()
        {
            GivenBookWithFiles("book.epub", "book.m4b");
            _trackFile.Path = Path.Combine(_rootPath, "book.azw3");

            var oldFiles = Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles;

            oldFiles.Should().HaveCount(1);
            oldFiles.Single().Path.Should().EndWith(".epub");

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_reuse_calibre_id_from_file_of_the_other_type()
        {
            GivenBookWithFiles("book.epub");
            _localTrack.Book.BookFiles.Value[0].CalibreId = 42;
            _trackFile.CalibreId = 0;
            _trackFile.Path = Path.Combine(_rootPath, "book.m4b");

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            _trackFile.CalibreId.Should().Be(42);
        }

        [Test]
        public void should_replace_an_audiobook_kept_under_its_own_root()
        {
            // The author's audiobooks live under a different root to the ebooks, so the file being
            // replaced is not below the author's ebook folder at all.
            var audiobookPath = @"C:\Test\Audiobooks\Author".AsOsAgnostic();

            _localTrack.Author.AudiobookPath = audiobookPath;
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(
                          new List<BookFile>
                          {
                              new BookFile { Id = 1, Path = Path.Combine(audiobookPath, "book.m4b") }
                          }))
                .Build();

            _trackFile.Path = Path.Combine(audiobookPath, "book.m4b");

            var oldFiles = Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles;

            oldFiles.Should().HaveCount(1);
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_throw_when_the_audiobook_folder_is_not_a_root_folder()
        {
            var audiobookPath = @"C:\Test\Audiobooks\Author".AsOsAgnostic();

            _localTrack.Author.AudiobookPath = audiobookPath;
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(
                          new List<BookFile>
                          {
                              new BookFile { Id = 1, Path = Path.Combine(audiobookPath, "book.m4b") }
                          }))
                .Build();

            _trackFile.Path = Path.Combine(audiobookPath, "book.m4b");

            // Nothing was ever added as a root folder covering the audiobook path.
            Mocker.GetMock<IRootFolderService>()
                .Setup(c => c.GetBestRootFolder(It.IsAny<string>()))
                .Returns((RootFolder)null);

            Subject.UpgradeBookFile(_trackFile, _localTrack).OldFiles.Should().HaveCount(1);
        }

        [Test]
        [Ignore("Pending readarr fix")]
        public void should_import_if_existing_file_doesnt_exist_in_db()
        {
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>())
                .Build();

            Subject.UpgradeBookFile(_trackFile, _localTrack);

            // Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localTrack.Book.BookFiles.Value, It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }
    }
}
