using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class ProfileRepositoryFixture : DbTest<QualityProfileRepository, QualityProfile>
    {
        [Test]
        public void should_be_able_to_read_and_write()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3, Quality.MP3, Quality.MP3),
                Cutoff = Quality.MP3.Id,
                Name = "TestProfile"
            };

            Subject.Insert(profile);

            StoredModel.Name.Should().Be(profile.Name);
            StoredModel.Cutoff.Should().Be(profile.Cutoff);

            StoredModel.Items.Should().Equal(profile.Items, (a, b) => a.Quality == b.Quality && a.Allowed == b.Allowed);
        }

        [Test]
        public void should_announce_a_change_to_a_profile()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.EPUB),
                Cutoff = Quality.EPUB.Id,
                Name = "TestProfile"
            };

            Subject.Insert(profile);

            profile.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3);
            Subject.Update(profile);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<ModelEvent<QualityProfile>>(e => e.Action == ModelAction.Updated && e.Model == profile)), Times.Once());
        }
    }
}
