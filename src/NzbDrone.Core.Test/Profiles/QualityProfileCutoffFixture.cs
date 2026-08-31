using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class QualityProfileCutoffFixture : CoreTest
    {
        private QualityProfile GivenProfile(int audiobookCutoff, params Quality[] allowed)
        {
            return new QualityProfile
            {
                Cutoff = Quality.AZW3.Id,
                AudiobookCutoff = audiobookCutoff,
                Items = Qualities.QualityFixture.GetDefaultQualities(allowed)
            };
        }

        [Test]
        public void should_use_the_ebook_cutoff_for_an_ebook_quality()
        {
            var profile = GivenProfile(Quality.FLAC.Id);

            profile.CutoffFor(Quality.EPUB).Should().Be(Quality.AZW3.Id);
        }

        [Test]
        public void should_use_the_audiobook_cutoff_for_an_audiobook_quality()
        {
            var profile = GivenProfile(Quality.FLAC.Id);

            profile.CutoffFor(Quality.MP3).Should().Be(Quality.FLAC.Id);
        }

        [Test]
        public void should_fall_back_to_the_first_allowed_audiobook_quality_when_unset()
        {
            var profile = GivenProfile(0, Quality.EPUB, Quality.MP3, Quality.FLAC);

            profile.CutoffFor(Quality.FLAC).Should().Be(Quality.MP3.Id);
        }

        [Test]
        public void should_fall_back_when_the_audiobook_cutoff_is_no_longer_allowed()
        {
            var profile = GivenProfile(Quality.FLAC.Id, Quality.EPUB, Quality.MP3);

            profile.CutoffFor(Quality.MP3).Should().Be(Quality.MP3.Id);
        }
    }
}
