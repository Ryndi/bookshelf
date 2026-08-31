using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(044)]
    public class add_audiobook_root_folder : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // AudiobookPath is built from this the same way Path is built from the author's
            // root folder. Empty keeps audiobooks beside the ebooks.
            Alter.Table("Authors").AddColumn("AudiobookRootFolderPath").AsString().Nullable();
        }
    }
}
