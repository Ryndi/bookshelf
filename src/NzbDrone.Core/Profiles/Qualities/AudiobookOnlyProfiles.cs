using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles.Qualities
{
    // An author whose profile takes no ebook quality at all wants audiobooks instead: an audiobook
    // library from before audiobooks had a setting of their own. Author.WantsEbooks and
    // Author.WantsAudiobooks decide it for one author; this is the same rule for queries over many,
    // shared so the Wanted list and the progress counts cannot disagree.
    public static class AudiobookOnlyProfiles
    {
        public static List<int> AudiobookOnlyProfileIds(this IDatabase database)
        {
            return database.Query<QualityProfile>(new SqlBuilder(database.DatabaseType))
                .Where(p => !p.Allows(false))
                .Select(p => p.Id)
                .ToList();
        }

        // True for an author judged by one of those profiles, for a query joining Authors.
        public static string AuthorIsAudiobookOnlySql(List<int> audiobookOnlyProfileIds)
        {
            return audiobookOnlyProfileIds.Any()
                ? $"\"Authors\".\"QualityProfileId\" IN ({string.Join(", ", audiobookOnlyProfileIds)})"
                : "1 = 0";
        }
    }
}
