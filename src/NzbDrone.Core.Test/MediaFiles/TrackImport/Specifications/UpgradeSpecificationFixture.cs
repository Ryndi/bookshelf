using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.BookImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private LocalBook _localBook;

        [SetUp]
        public void Setup()
        {
            var author = Builder<Author>.CreateNew()
                .With(a => a.QualityProfile = new QualityProfile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();

            _localBook = new LocalBook
            {
                Path = @"C:\Test\Author\book.epub".AsOsAgnostic(),
                Quality = new QualityModel(Quality.EPUB, new Revision(version: 1)),
                Author = author,
                Book = Builder<Book>.CreateNew().Build()
            };
        }

        private void GivenExistingFiles(params QualityModel[] qualities)
        {
            _localBook.Book.BookFiles = new LazyLoaded<List<BookFile>>(
                qualities.Select(q => new BookFile { Quality = q }).ToList());
        }

        [Test]
        public void should_accept_when_the_book_has_no_existing_files()
        {
            GivenExistingFiles();

            Subject.IsSatisfiedBy(_localBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_an_ebook_when_only_an_audiobook_is_on_disk()
        {
            GivenExistingFiles(new QualityModel(Quality.MP3, new Revision(version: 1)));

            Subject.IsSatisfiedBy(_localBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_a_downgrade_within_the_same_format()
        {
            GivenExistingFiles(new QualityModel(Quality.AZW3, new Revision(version: 1)));

            Subject.IsSatisfiedBy(_localBook, null).Accepted.Should().BeFalse();
        }

        /*
        private Author _author;
        private Book _book;
        private LocalTrack _localTrack;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                     }).Build();

            _book = Builder<Book>.CreateNew().Build();

            _localTrack = new LocalTrack
            {
                Path = @"C:\Test\Imagine Dragons\Imagine.Dragons.Song.1.mp3",
                Quality = new QualityModel(Quality.MP3, new Revision(version: 1)),
                Author = _author,
                Book = _book
            };
        }

        [Test]
        public void should_return_true_if_no_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 0)
                                                     .With(e => e.TrackFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_no_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 0)
                                                     .With(e => e.TrackFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_one_existing_trackFile_for_multi_track()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3, new Revision(version: 1))
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.TrackFileId = 2)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.MP3, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.MP3, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localTrack.Quality = new QualityModel(Quality.FLAC);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.FLAC, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_track_file_is_null()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }
        */
    }
}
