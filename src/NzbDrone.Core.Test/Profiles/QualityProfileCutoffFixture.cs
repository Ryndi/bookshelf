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

        [Test]
        public void should_keep_the_cutoff_an_audiobook_only_profile_already_had()
        {
            // Written before audiobooks had a cutoff of their own, so it is held in Cutoff. Falling
            // back to the first allowed quality would stop every upgrade at Unknown Audio.
            var profile = GivenProfile(0, Quality.UnknownAudio, Quality.MP3, Quality.M4B, Quality.FLAC);
            profile.Cutoff = Quality.M4B.Id;

            profile.CutoffFor(Quality.MP3).Should().Be(Quality.M4B.Id);
        }

        [Test]
        public void should_have_no_audiobook_cutoff_when_the_profile_takes_no_audiobooks()
        {
            var profile = GivenProfile(0, Quality.EPUB, Quality.AZW3);

            profile.CutoffFor(true).Should().Be(0);
        }

        [Test]
        public void should_not_throw_when_the_profile_allows_nothing()
        {
            // The schema a new profile starts from allows nothing yet, and the API asks it for
            // its audiobook cutoff while building the response.
            var profile = GivenProfile(0);

            foreach (var item in profile.Items)
            {
                item.Allowed = false;
            }

            profile.CutoffFor(true).Should().Be(0);
        }
    }
}
