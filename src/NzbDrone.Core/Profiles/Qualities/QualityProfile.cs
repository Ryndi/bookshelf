using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfile : ModelBase
    {
        public QualityProfile()
        {
            FormatItems = new List<ProfileFormatItem>();
        }

        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public int Cutoff { get; set; }
        public int AudiobookCutoff { get; set; }
        public int MinFormatScore { get; set; }
        public int CutoffFormatScore { get; set; }
        public List<ProfileFormatItem> FormatItems { get; set; }
        public List<QualityProfileQualityItem> Items { get; set; }

        public Quality FirstAllowedQuality()
        {
            var firstAllowed = Items.First(q => q.Allowed);

            if (firstAllowed.Quality != null)
            {
                return firstAllowed.Quality;
            }

            // Returning any item from the group will work,
            // returning the first because it's the true first quality.
            return firstAllowed.Items.First().Quality;
        }

        // Audiobooks and ebooks are never upgrades of each other, so each format stops at its
        // own cutoff. Profiles written before AudiobookCutoff existed fall back to the first
        // allowed audiobook quality, which stops at the first acceptable file rather than
        // chasing an ebook cutoff an audiobook can never reach.
        public int CutoffFor(Quality quality)
        {
            return CutoffFor(Quality.IsAudio(quality));
        }

        public int CutoffFor(bool audio)
        {
            if (!audio)
            {
                return Cutoff;
            }

            if (AudiobookCutoff > 0 && IsAllowedCutoff(AudiobookCutoff))
            {
                return AudiobookCutoff;
            }

            return FirstAllowedQuality(true).Id;
        }

        public Quality FirstAllowedQuality(bool audio)
        {
            var first = Items.Where(q => q.Allowed)
                .SelectMany(q => q.GetQualities())
                .FirstOrDefault(q => Quality.IsAudio(q) == audio);

            return first ?? FirstAllowedQuality();
        }

        private bool IsAllowedCutoff(int id)
        {
            return Items.Any(i => i.Allowed &&
                                  ((i.Id > 0 && i.Id == id) || i.GetQualities().Any(q => q.Id == id)));
        }

        public Quality LastAllowedQuality()
        {
            var lastAllowed = Items.Last(q => q.Allowed);

            if (lastAllowed.Quality != null)
            {
                return lastAllowed.Quality;
            }

            // Returning any item from the group will work,
            // returning the last because it's the true last quality.
            return lastAllowed.Items.Last().Quality;
        }

        public QualityIndex GetIndex(Quality quality, bool respectGroupOrder = false)
        {
            return GetIndex(quality.Id, respectGroupOrder);
        }

        public QualityIndex GetIndex(int id, bool respectGroupOrder = false)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var quality = item.Quality;

                // Quality matches by ID
                if (quality != null && quality.Id == id)
                {
                    return new QualityIndex(i);
                }

                // Group matches by ID
                if (item.Id > 0 && item.Id == id)
                {
                    return new QualityIndex(i);
                }

                for (var g = 0; g < item.Items.Count; g++)
                {
                    var groupItem = item.Items[g];

                    if (groupItem.Quality.Id == id)
                    {
                        return respectGroupOrder ? new QualityIndex(i, g) : new QualityIndex(i);
                    }
                }
            }

            return new QualityIndex();
        }

        public int CalculateCustomFormatScore(List<CustomFormat> formats)
        {
            return FormatItems.Where(x => formats.Contains(x.Format)).Sum(x => x.Score);
        }
    }
}
