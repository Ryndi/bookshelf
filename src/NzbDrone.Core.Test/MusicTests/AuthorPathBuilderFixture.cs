using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.BookTests
{
    [TestFixture]
    public class AuthorPathBuilderFixture : CoreTest<AuthorPathBuilder>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                .With(a => a.Path = @"C:\Books\Some Author".AsOsAgnostic())
                .With(a => a.AudiobookRootFolderPath = null)
                .Build();

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns("Some Author");
        }

        [Test]
        public void should_return_null_when_no_audiobook_root_is_set()
        {
            Subject.BuildAudiobookPath(_author).Should().BeNull();
        }

        [Test]
        public void should_build_the_author_folder_under_the_audiobook_root()
        {
            _author.AudiobookRootFolderPath = @"C:\Audiobooks".AsOsAgnostic();

            Subject.BuildAudiobookPath(_author)
                .Should().Be(Path.Combine(@"C:\Audiobooks".AsOsAgnostic(), "Some Author"));
        }

        [Test]
        public void should_use_the_configured_author_folder_naming()
        {
            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns("Author, Some");

            _author.AudiobookRootFolderPath = @"C:\Audiobooks".AsOsAgnostic();

            Subject.BuildAudiobookPath(_author)
                .Should().Be(Path.Combine(@"C:\Audiobooks".AsOsAgnostic(), "Author, Some"));
        }

        [Test]
        public void should_keep_the_folder_the_author_already_has()
        {
            // The author has been renamed since, but the audiobooks already on disk stay where they are.
            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns("Renamed Author");

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                .Returns(@"C:\Audiobooks".AsOsAgnostic());

            _author.AudiobookRootFolderPath = @"C:\Audiobooks".AsOsAgnostic();

            Subject.BuildAudiobookPath(_author, Path.Combine(@"C:\Audiobooks".AsOsAgnostic(), "Some Author"))
                .Should().Be(Path.Combine(@"C:\Audiobooks".AsOsAgnostic(), "Some Author"));
        }

        [Test]
        public void should_keep_the_folder_name_under_a_new_root()
        {
            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                .Returns(@"C:\Audiobooks".AsOsAgnostic());

            _author.AudiobookRootFolderPath = @"D:\Audiobooks".AsOsAgnostic();

            Subject.BuildAudiobookPath(_author, Path.Combine(@"C:\Audiobooks".AsOsAgnostic(), "Some Author"))
                .Should().Be(Path.Combine(@"D:\Audiobooks".AsOsAgnostic(), "Some Author"));
        }
    }
}
