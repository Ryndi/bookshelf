using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.BookTests
{
    [TestFixture]
    public class AudiobookFolderHandlerFixture : CoreTest<AudiobookFolderHandler>
    {
        private Author _author;
        private string _root = @"C:\Audiobooks".AsOsAgnostic();
        private string _folder = @"C:\Audiobooks\Some Author".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                .With(a => a.SearchAudiobooks = true)
                .With(a => a.AudiobookPath = _folder)
                .Build();

            GivenFolderExists(_root, true);
            GivenFolderExists(_folder, false);
        }

        private void GivenFolderExists(string path, bool exists)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(path))
                .Returns(exists);
        }

        private void VerifyCreated(Times times)
        {
            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CreateFolder(_folder), times);
        }

        [Test]
        public void should_create_the_folder_when_audiobooks_are_enabled()
        {
            Subject.Handle(new AuthorEditedEvent(_author, _author));

            VerifyCreated(Times.Once());
        }

        [Test]
        public void should_create_the_folder_when_the_author_is_added()
        {
            Subject.Handle(new AuthorAddedEvent(_author));

            VerifyCreated(Times.Once());
        }

        [Test]
        public void should_not_create_anything_when_audiobooks_are_off()
        {
            _author.SearchAudiobooks = false;

            Subject.Handle(new AuthorEditedEvent(_author, _author));

            VerifyCreated(Times.Never());
        }

        [Test]
        public void should_not_create_anything_when_no_audiobook_path_is_set()
        {
            _author.AudiobookPath = null;

            Subject.Handle(new AuthorEditedEvent(_author, _author));

            VerifyCreated(Times.Never());
        }

        [Test]
        public void should_do_nothing_when_the_folder_already_exists()
        {
            GivenFolderExists(_folder, true);

            Subject.Handle(new AuthorEditedEvent(_author, _author));

            VerifyCreated(Times.Never());
        }

        [Test]
        public void should_not_create_the_root_itself_when_it_is_missing()
        {
            GivenFolderExists(_root, false);

            Subject.Handle(new AuthorEditedEvent(_author, _author));

            VerifyCreated(Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CreateFolder(_root), Times.Never());
        }
    }
}
