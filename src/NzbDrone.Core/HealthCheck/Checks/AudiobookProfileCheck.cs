using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.HealthCheck.Checks
{
    // An author wanting audiobooks has every one turned away when the profile judging them takes no
    // audio quality, and its books stay on the Missing list for good. Saving an author like that is
    // refused, but one left that way by the bulk editor, or by a change to the profile, is flagged here.
    [CheckOn(typeof(AuthorAddedEvent))]
    [CheckOn(typeof(AuthorEditedEvent))]
    [CheckOn(typeof(AuthorBulkEditedEvent))]
    [CheckOn(typeof(AuthorDeletedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(ModelEvent<QualityProfile>))]
    public class AudiobookProfileCheck : HealthCheckBase
    {
        private readonly IAuthorService _authorService;
        private readonly IQualityProfileService _qualityProfileService;

        public AudiobookProfileCheck(IAuthorService authorService, IQualityProfileService qualityProfileService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _authorService = authorService;
            _qualityProfileService = qualityProfileService;
        }

        public override HealthCheck Check()
        {
            var profiles = _qualityProfileService.All().ToDictionary(p => p.Id);

            var authors = _authorService.GetAllAuthors()
                                        .Where(a => a.SearchAudiobooks && !TakesAudiobooks(a, profiles))
                                        .ToList();

            if (authors.Any())
            {
                var message = string.Format(_localizationService.GetLocalizedString("AudiobookProfileCheckMessage"), string.Join(", ", authors.Select(a => a.Name)));

                return new HealthCheck(GetType(), HealthCheckResult.Warning, message);
            }

            return new HealthCheck(GetType());
        }

        // Audiobooks are judged by the audiobook profile when one is set, and the main one otherwise -
        // the way Author.ProfileFor picks it.
        private static bool TakesAudiobooks(Author author, Dictionary<int, QualityProfile> profiles)
        {
            var profileId = author.AudiobookQualityProfileId > 0 && profiles.ContainsKey(author.AudiobookQualityProfileId)
                ? author.AudiobookQualityProfileId
                : author.QualityProfileId;

            return profiles.TryGetValue(profileId, out var profile) && profile.Allows(true);
        }
    }
}
