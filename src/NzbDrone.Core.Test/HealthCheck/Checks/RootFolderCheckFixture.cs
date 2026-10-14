using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Localization;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RootFolderCheckFixture : CoreTest<RootFolderCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private void GivenMissingRootFolder(string rootFolderPath)
        {
            var author = Builder<Author>.CreateListOfSize(1)
                                        .All()
                                        .With(a => a.AudiobookPath = null)
                                        .Build()
                                        .ToList();

            var importList = Builder<ImportListDefinition>.CreateListOfSize(1)
                .Build()
                .ToList();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.AllAuthorPaths())
                  .Returns(author.ToDictionary(x => x.Id, x => x.Path));

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAllAuthors())
                  .Returns(author);

            Mocker.GetMock<IImportListFactory>()
                .Setup(s => s.All())
                .Returns(importList);

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(rootFolderPath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);
        }

        [Test]
        public void should_not_return_error_when_no_book()
        {
            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.AllAuthorPaths())
                  .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAllAuthors())
                  .Returns(new List<Author>());

            Mocker.GetMock<IImportListFactory>()
                .Setup(s => s.All())
                .Returns(new List<ImportListDefinition>());

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_if_the_root_of_an_authors_audiobooks_is_missing()
        {
            var author = Builder<Author>.CreateNew()
                                        .With(a => a.Id = 1)
                                        .With(a => a.Path = @"C:\Books\Author".AsOsAgnostic())
                                        .With(a => a.AudiobookPath = @"D:\Audiobooks\Author".AsOsAgnostic())
                                        .Build();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.AllAuthorPaths())
                  .Returns(new Dictionary<int, string> { { author.Id, author.Path } });

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAllAuthors())
                  .Returns(new List<Author> { author });

            Mocker.GetMock<IImportListFactory>()
                  .Setup(s => s.All())
                  .Returns(new List<ImportListDefinition>());

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(author.Path))
                  .Returns(@"C:\Books".AsOsAgnostic());

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(author.AudiobookPath))
                  .Returns(@"D:\Audiobooks".AsOsAgnostic());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(@"C:\Books".AsOsAgnostic()))
                  .Returns(true);

            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString("RootFolderCheckSingleMessage"))
                  .Returns("Missing root folder: {0}");

            Subject.Check().ShouldBeError(@"D:\Audiobooks".AsOsAgnostic());
        }

        [Test]
        public void should_return_error_if_book_parent_is_missing()
        {
            GivenMissingRootFolder(@"C:\Books".AsOsAgnostic());

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_series_path_is_for_posix_os()
        {
            WindowsOnly();
            GivenMissingRootFolder("/mnt/books");

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_series_path_is_for_windows()
        {
            PosixOnly();
            GivenMissingRootFolder(@"C:\Books");

            Subject.Check().ShouldBeError();
        }
    }
}
