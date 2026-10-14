using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteAuthorFilesFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private Author _author;
        private List<Author> _otherAuthors;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Test\Books\Author Name".AsOsAgnostic())
                .With(a => a.AudiobookPath = @"C:\Test\Audiobooks\Author Name".AsOsAgnostic())
                .Build();

            _otherAuthors = new List<Author>();

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolder(It.IsAny<string>()))
                .Returns(new RootFolder());

            Mocker.GetMock<IAuthorService>()
                .Setup(s => s.AllAuthorPaths())
                .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IAuthorService>()
                .Setup(s => s.GetAllAuthors())
                .Returns(_otherAuthors);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(It.IsAny<string>()))
                .Returns(true);
        }

        [Test]
        public void should_recycle_the_audiobook_folder_along_with_the_author()
        {
            Subject.HandleAsync(new AuthorDeletedEvent(_author, true, false));

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFolder(_author.Path), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFolder(_author.AudiobookPath), Times.Once());
        }

        [Test]
        public void should_not_recycle_an_audiobook_folder_another_author_uses()
        {
            _otherAuthors.Add(Builder<Author>.CreateNew()
                .With(a => a.Id = _author.Id + 1)
                .With(a => a.Path = @"C:\Test\Books\Someone Else".AsOsAgnostic())
                .With(a => a.AudiobookPath = _author.AudiobookPath)
                .Build());

            Subject.HandleAsync(new AuthorDeletedEvent(_author, true, false));

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFolder(_author.AudiobookPath), Times.Never());
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_leave_the_audiobook_folder_when_files_are_kept()
        {
            Subject.HandleAsync(new AuthorDeletedEvent(_author, false, false));

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFolder(It.IsAny<string>()), Times.Never());
        }
    }
}
