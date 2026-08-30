using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    // A book may monitor one edition per format, so the number of monitored editions is bounded
    // where they are written rather than here. All that is repaired is a book left with none.
    public class FixUnmonitoredEditions : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public FixUnmonitoredEditions(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                mapper.Execute(@"UPDATE ""Editions""
                                SET ""Monitored"" = true
                                WHERE ""Id"" IN (
                                    SELECT MIN(""Id"")
                                    FROM ""Editions""
                                    WHERE ""BookId"" NOT IN (
                                        SELECT ""BookId"" FROM ""Editions"" WHERE ""Monitored"" = true
                                    )
                                    GROUP BY ""BookId""
                                )");
            }
            else
            {
                mapper.Execute(@"UPDATE ""Editions""
                                SET ""Monitored"" = 1
                                WHERE ""Id"" IN (
                                    SELECT MIN(""Id"")
                                    FROM ""Editions""
                                    WHERE ""BookId"" NOT IN (
                                        SELECT ""BookId"" FROM ""Editions"" WHERE ""Monitored"" = 1
                                    )
                                    GROUP BY ""BookId""
                                )");
            }
        }
    }
}
