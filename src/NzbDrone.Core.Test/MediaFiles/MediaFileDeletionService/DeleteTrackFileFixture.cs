using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteTrackFileFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private static readonly string RootFolder = @"C:\Test\Music";
        private Author _author;
        private BookFile _trackFile;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                                     .With(s => s.Path = Path.Combine(RootFolder, "Author Name"))
                                     .Build();

            _trackFile = Builder<BookFile>.CreateNew()
                                               .With(f => f.Path = "/Author Name - Track01")
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_author.Path))
                  .Returns(RootFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_trackFile.Path))
                  .Returns(_author.Path);
        }

        private void GivenRootFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(RootFolder))
                  .Returns(true);
        }

        private void GivenRootFolderHasFolders()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(RootFolder))
                  .Returns(new[] { _author.Path });
        }

        private void GivenAuthorFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_author.Path))
                  .Returns(true);
        }

        private void GivenNonCalibreRootFolder()
        {
            Mocker.GetMock<IRootFolderService>()
                .Setup(x => x.GetBestRootFolder(It.IsAny<string>()))
                .Returns(new RootFolder());
        }

        [Test]
        public void should_throw_if_root_folder_does_not_exist()
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_throw_if_root_folder_is_empty()
        {
            GivenRootFolderExists();
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_from_db_if_author_folder_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_db_if_track_file_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_disk_and_db_if_track_file_exists()
        {
            GivenNonCalibreRootFolder();
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Author Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
        }

        private BookFile GivenAudiobookUnderItsOwnRoot()
        {
            var audiobookRoot = @"C:\Test\Audiobooks".AsOsAgnostic();
            _author.AudiobookPath = Path.Combine(audiobookRoot, "Author Name");

            var bookFolder = Path.Combine(_author.AudiobookPath, "Book");
            var audiobook = Builder<BookFile>.CreateNew()
                .With(f => f.Path = Path.Combine(bookFolder, "Book.m4b"))
                .Build();

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(_author.AudiobookPath)).Returns(audiobookRoot);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(audiobook.Path)).Returns(bookFolder);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(audiobookRoot)).Returns(true);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetDirectories(audiobookRoot)).Returns(new[] { _author.AudiobookPath });
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(_author.AudiobookPath)).Returns(true);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FileExists(audiobook.Path)).Returns(true);

            return audiobook;
        }

        [Test]
        public void should_delete_an_audiobook_kept_under_its_own_root_folder()
        {
            GivenNonCalibreRootFolder();
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            var audiobook = GivenAudiobookUnderItsOwnRoot();

            Subject.DeleteTrackFile(_author, audiobook);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(audiobook.Path, Path.Combine("Author Name", "Book")), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(audiobook, DeleteMediaFileReason.Manual), Times.Once());
        }

        [Test]
        public void should_delete_an_audiobook_when_the_author_has_no_ebook_folder()
        {
            // The author folder under the ebook root does not exist. Judged by that, the row would be
            // removed and the file left behind, to be imported again by the next rescan.
            GivenNonCalibreRootFolder();
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            var audiobook = GivenAudiobookUnderItsOwnRoot();

            Subject.DeleteTrackFile(_author, audiobook);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(audiobook.Path, Path.Combine("Author Name", "Book")), Times.Once());
        }

        [Test]
        public void should_handle_error_deleting_track_file()
        {
            GivenNonCalibreRootFolder();
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Setup(s => s.DeleteFile(_trackFile.Path, "Author Name"))
                  .Throws(new IOException());

            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));

            ExceptionVerification.ExpectedErrors(1);
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Author Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Never());
        }
    }
}
