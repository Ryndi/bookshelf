using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Books
{
    public interface IBookCutoffService
    {
        PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec);
    }

    public class BookCutoffService : IBookCutoffService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IQualityProfileService _qualityProfileService;

        public BookCutoffService(IBookRepository bookRepository, IQualityProfileService qualityProfileService)
        {
            _bookRepository = bookRepository;
            _qualityProfileService = qualityProfileService;
        }

        public PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _qualityProfileService.All();

            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                // Each format has its own cutoff, so a quality only counts as below cutoff when
                // it is below the cutoff set for its own format. The two are kept apart because an
                // author can judge audiobooks by a different profile from ebooks.
                var ebookIds = BelowCutoff(profile, false);
                var audiobookIds = BelowCutoff(profile, true);

                if (ebookIds.Any() || audiobookIds.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, ebookIds, audiobookIds));
                }
            }

            if (qualitiesBelowCutoff.Empty())
            {
                pagingSpec.Records = new List<Book>();

                return pagingSpec;
            }

            return _bookRepository.BooksWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }

        private static List<int> BelowCutoff(QualityProfile profile, bool audio)
        {
            var cutoff = profile.UpgradeAllowed ? profile.CutoffFor(audio) : profile.FirstAllowedQuality(audio).Id;
            var cutoffIndex = profile.GetIndex(cutoff);

            return profile.Items.Take(cutoffIndex.Index)
                .SelectMany(i => i.GetQualities())
                .Where(q => Quality.IsAudio(q) == audio)
                .Select(q => q.Id)
                .Distinct()
                .ToList();
        }
    }
}
