using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(043)]
    public class add_audiobook_path : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Empty keeps audiobooks beside the ebooks in the author's existing folder.
            Alter.Table("Authors").AddColumn("AudiobookPath").AsString().Nullable();
        }
    }
}
