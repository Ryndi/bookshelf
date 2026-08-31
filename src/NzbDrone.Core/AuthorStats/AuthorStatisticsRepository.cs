using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.AuthorStats
{
    public interface IAuthorStatisticsRepository
    {
        List<BookStatistics> AuthorStatistics();
        List<BookStatistics> AuthorStatistics(int authorId);
    }

    public class AuthorStatisticsRepository : IAuthorStatisticsRepository
    {
        private const string _selectTemplate = "SELECT /**select**/ FROM \"Editions\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public AuthorStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<BookStatistics> AuthorStatistics()
        {
            return Query(Builder());
        }

        public List<BookStatistics> AuthorStatistics(int authorId)
        {
            return Query(Builder().Where<Author>(x => x.Id == authorId));
        }

        private List<BookStatistics> Query(SqlBuilder builder)
        {
            var sql = builder.AddTemplate(_selectTemplate).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<BookStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder Builder()
        {
            var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";
            var counted = $@"(""Books"".""Monitored"" = {trueIndicator} AND (""Books"".""ReleaseDate"" < @currentDate) OR ""Books"".""ReleaseDate"" IS NULL) OR MIN(""BookFiles"".""Id"") IS NOT NULL";
            var wantsAudiobooks = $@"COALESCE(""Books"".""SearchAudiobooks"", ""Authors"".""SearchAudiobooks"") = {trueIndicator}";

            return new SqlBuilder(_database.DatabaseType)
            .Select($@"""Authors"".""Id"" AS ""AuthorId"",
                     ""Books"".""Id"" AS ""BookId"",
                     SUM(COALESCE(""BookFiles"".""Size"", 0)) AS ""SizeOnDisk"",
                     1 AS ""TotalBookCount"",
                     CASE WHEN MIN(""BookFiles"".""Id"") IS NULL THEN 0 ELSE 1 END AS ""AvailableBookCount"",
                     CASE WHEN {counted} THEN 1 ELSE 0 END AS ""BookCount"",
                     CASE WHEN MIN(""BookFiles"".""Id"") IS NULL THEN 0 ELSE COUNT(""BookFiles"".""Id"") END AS ""BookFileCount"",
                     CASE WHEN EXISTS {HasFileOfFormatSubquery(false)} THEN 1 ELSE 0 END AS ""AvailableEbookCount"",
                     CASE WHEN {wantsAudiobooks} AND ({counted}) THEN 1 ELSE 0 END AS ""AudiobookCount"",
                     CASE WHEN {wantsAudiobooks} AND EXISTS {HasFileOfFormatSubquery(true)} THEN 1 ELSE 0 END AS ""AvailableAudiobookCount""")
            .Join<Edition, Book>((e, b) => e.BookId == b.Id)
            .Join<Book, Author>((book, author) => book.AuthorMetadataId == author.AuthorMetadataId)
            .LeftJoin<Edition, BookFile>((t, f) => t.Id == f.EditionId)
            .Where<Edition>(x => x.Monitored == true)
            .GroupBy<Author>(x => x.Id)
            .GroupBy<Book>(x => x.Id)
            .AddParameters(new Dictionary<string, object> { { "currentDate", DateTime.UtcNow } });
        }

        // Matches how BookRepository decides a book still wants a format, so the progress the bar
        // shows and the books the search considers missing cannot disagree. Aliased because the
        // outer query already has BookFiles joined.
        private string HasFileOfFormatSubquery(bool audio)
        {
            var qualityMatch = string.Join(
                " OR ",
                Quality.All.Where(q => Quality.IsAudio(q) == audio)
                    .Select(q => $"\"FormatFiles\".\"Quality\" LIKE '%_quality_: {q.Id},%'"));

            return "(SELECT 1 FROM \"BookFiles\" AS \"FormatFiles\" " +
                   "JOIN \"Editions\" AS \"FormatEditions\" ON \"FormatFiles\".\"EditionId\" = \"FormatEditions\".\"Id\" " +
                   $"WHERE \"FormatEditions\".\"BookId\" = \"Books\".\"Id\" AND ({qualityMatch}))";
        }
    }
}
