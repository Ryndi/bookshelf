using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Qualities
{
    public class QualitiesBelowCutoff
    {
        public int ProfileId { get; set; }
        public IEnumerable<int> QualityIds { get; set; }

        // Audio files are judged by an author's audiobook profile when one is set, so these are
        // matched against authors using this profile for audiobooks rather than as their main one.
        public IEnumerable<int> AudiobookQualityIds { get; set; }

        public QualitiesBelowCutoff(int profileId, IEnumerable<int> qualityIds, IEnumerable<int> audiobookQualityIds = null)
        {
            ProfileId = profileId;
            QualityIds = qualityIds;
            AudiobookQualityIds = audiobookQualityIds ?? Enumerable.Empty<int>();
        }
    }
}
