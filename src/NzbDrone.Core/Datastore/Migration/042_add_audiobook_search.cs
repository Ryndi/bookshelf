using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(042)]
    public class add_audiobook_search : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Zero means the author is not tracking audiobooks, so existing libraries keep
            // behaving exactly as before until the setting is turned on.
            Alter.Table("Authors").AddColumn("AudiobookQualityProfileId").AsInt32().WithDefaultValue(0);
            Alter.Table("Authors").AddColumn("SearchAudiobooks").AsBoolean().WithDefaultValue(false);

            // Null inherits the author's setting.
            Alter.Table("Books").AddColumn("SearchAudiobooks").AsBoolean().Nullable();
        }
    }
}
