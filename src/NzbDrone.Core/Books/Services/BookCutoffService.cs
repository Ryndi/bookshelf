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
                var ids = new List<int>();

                // Each format has its own cutoff, so a quality only counts as below cutoff when
                // it is below the cutoff set for its own format.
                foreach (var isAudio in new[] { false, true })
                {
                    var cutoff = profile.UpgradeAllowed ? profile.CutoffFor(isAudio) : profile.FirstAllowedQuality(isAudio).Id;
                    var cutoffIndex = profile.GetIndex(cutoff);

                    ids.AddRange(profile.Items.Take(cutoffIndex.Index)
                        .SelectMany(i => i.GetQualities())
                        .Where(q => Quality.IsAudio(q) == isAudio)
                        .Select(q => q.Id));
                }

                if (ids.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, ids.Distinct()));
                }
            }

            if (qualitiesBelowCutoff.Empty())
            {
                pagingSpec.Records = new List<Book>();

                return pagingSpec;
            }

            return _bookRepository.BooksWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }
    }
}
