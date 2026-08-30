using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.BookTests.EditionRepositoryTests
{
    [TestFixture]
    public class EditionRepositoryFixture : DbTest<EditionRepository, Edition>
    {
        private Book _book;
        private int _editionCount;

        [SetUp]
        public void Setup()
        {
            _editionCount = 0;

            var meta = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);

            _book = Builder<Book>.CreateNew()
                .With(b => b.Id = 0)
                .With(b => b.AuthorMetadataId = meta.Id)
                .Build();
            Db.Insert(_book);
        }

        private Edition GivenEdition(string format, bool monitored)
        {
            _editionCount++;

            var edition = Builder<Edition>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.BookId = _book.Id)
                .With(e => e.ForeignEditionId = $"edition{_editionCount}")
                .With(e => e.TitleSlug = $"slug{_editionCount}")
                .With(e => e.Format = format)
                .With(e => e.Monitored = monitored)
                .Build();

            Db.Insert(edition);

            return edition;
        }

        [Test]
        public void should_keep_audiobook_monitored_when_monitoring_an_ebook()
        {
            var audiobook = GivenEdition("Audiobook", true);
            var ebook = GivenEdition("Kindle Edition", false);

            var result = Subject.SetMonitored(ebook);

            result.Single(x => x.Id == ebook.Id).Monitored.Should().BeTrue();
            result.Single(x => x.Id == audiobook.Id).Monitored.Should().BeTrue();
        }

        [Test]
        public void should_keep_ebook_monitored_when_monitoring_an_audiobook()
        {
            var ebook = GivenEdition("Kindle Edition", true);
            var audiobook = GivenEdition("Audible Audio", false);

            var result = Subject.SetMonitored(audiobook);

            result.Single(x => x.Id == ebook.Id).Monitored.Should().BeTrue();
            result.Single(x => x.Id == audiobook.Id).Monitored.Should().BeTrue();
        }

        [Test]
        public void should_unmonitor_the_other_edition_of_the_same_format()
        {
            var first = GivenEdition("Kindle Edition", true);
            var second = GivenEdition("Nook", false);

            var result = Subject.SetMonitored(second);

            result.Single(x => x.Id == first.Id).Monitored.Should().BeFalse();
            result.Single(x => x.Id == second.Id).Monitored.Should().BeTrue();
        }

        [Test]
        public void should_group_an_edition_of_unknown_format_with_ebooks()
        {
            var unknown = GivenEdition(null, true);
            var audiobook = GivenEdition("Audiobook", false);

            var result = Subject.SetMonitored(audiobook);

            result.Single(x => x.Id == unknown.Id).Monitored.Should().BeTrue();

            result = Subject.SetMonitored(unknown);

            result.Single(x => x.Id == audiobook.Id).Monitored.Should().BeTrue();
            result.Single(x => x.Id == unknown.Id).Monitored.Should().BeTrue();
        }
    }
}
