using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.AuthorStatsTests
{
    [TestFixture]
    public class AuthorStatisticsServiceFixture : CoreTest<AuthorStatisticsService>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                .With(a => a.Id = 1)
                .Build();

            Mocker.GetMock<IAuthorStatisticsRepository>()
                  .Setup(s => s.AuthorStatistics(_author.Id))
                  .Returns(() => new List<BookStatistics> { new BookStatistics { AuthorId = _author.Id } });
        }

        private void ShouldHaveReadTheStatistics(int times)
        {
            Mocker.GetMock<IAuthorStatisticsRepository>()
                  .Verify(v => v.AuthorStatistics(_author.Id), Times.Exactly(times));
        }

        [Test]
        public void should_keep_the_statistics_until_something_changes()
        {
            Subject.AuthorStatistics(_author.Id);
            Subject.AuthorStatistics(_author.Id);

            ShouldHaveReadTheStatistics(1);
        }

        [Test]
        public void should_read_the_statistics_again_after_the_author_is_edited()
        {
            Subject.AuthorStatistics(_author.Id);

            Subject.Handle(new AuthorEditedEvent(_author, _author));
            Subject.AuthorStatistics(_author.Id);

            ShouldHaveReadTheStatistics(2);
        }

        [Test]
        public void should_read_the_statistics_again_after_the_author_is_edited_in_bulk()
        {
            Subject.AuthorStatistics(_author.Id);

            Subject.Handle(new AuthorBulkEditedEvent(new List<Author> { _author }));
            Subject.AuthorStatistics(_author.Id);

            ShouldHaveReadTheStatistics(2);
        }

        [Test]
        public void should_read_the_statistics_again_after_a_quality_profile_changes()
        {
            Subject.AuthorStatistics(_author.Id);

            Subject.Handle(new ModelEvent<QualityProfile>(new QualityProfile { Id = 1 }, ModelAction.Updated));
            Subject.AuthorStatistics(_author.Id);

            ShouldHaveReadTheStatistics(2);
        }
    }
}
