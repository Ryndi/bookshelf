using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Extras.Files
{
    public abstract class ExtraFile : ModelBase
    {
        public int AuthorId { get; set; }
        public int? BookFileId { get; set; }
        public int? BookId { get; set; }

        // Relative to the author's folder, or the full path of an extra outside it - see ExtraFilePaths.
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {RelativePath}";
        }
    }
}
