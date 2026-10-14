using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class AudiobookProfileCheckFixture : CoreTest<AudiobookProfileCheck>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("No audio quality for: {0}");

            Mocker.GetMock<IQualityProfileService>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityProfile>
                  {
                      new QualityProfile { Id = 1, Name = "eBook", Items = Qualities.QualityFixture.GetDefaultQualities(Quality.EPUB) },
                      new QualityProfile { Id = 2, Name = "Spoken", Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3) }
                  });

            _author = Builder<Author>.CreateNew()
                                     .With(a => a.Name = "Some Author")
                                     .With(a => a.QualityProfileId = 1)
                                     .With(a => a.AudiobookQualityProfileId = 0)
                                     .With(a => a.SearchAudiobooks = true)
                                     .Build();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAllAuthors())
                  .Returns(new List<Author> { _author });
        }

        [Test]
        public void should_warn_when_audiobooks_are_judged_by_a_profile_that_takes_no_audio()
        {
            Subject.Check().ShouldBeWarning("Some Author");
        }

        [Test]
        public void should_be_ok_when_the_audiobook_profile_takes_audio()
        {
            _author.AudiobookQualityProfileId = 2;

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_be_ok_when_audiobooks_are_not_searched_for()
        {
            _author.SearchAudiobooks = false;

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_judge_by_the_main_profile_when_the_audiobook_profile_is_gone()
        {
            _author.QualityProfileId = 2;
            _author.AudiobookQualityProfileId = 3;

            Subject.Check().ShouldBeOk();
        }
    }
}
