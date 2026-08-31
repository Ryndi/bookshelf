using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(041)]
    public class add_audiobook_cutoff : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Zero means "not set"; QualityProfile falls back to the first allowed audiobook
            // quality so existing profiles keep working until the user picks one.
            Alter.Table("QualityProfiles").AddColumn("AudiobookCutoff").AsInt32().WithDefaultValue(0);
        }
    }
}
