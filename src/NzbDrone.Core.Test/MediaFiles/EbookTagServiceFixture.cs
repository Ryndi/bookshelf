using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using VersOne.Epub.Schema;

namespace NzbDrone.Core.Test.MediaFiles.AudioTagServiceFixture
{
    [TestFixture]
    public class EbookTagServiceFixture : CoreTest<EBookTagService>
    {
        [Test]
        public void should_prefer_isbn13()
        {
            var ids = Builder<EpubMetadataIdentifier>
                .CreateListOfSize(2)
                .TheFirst(1)
                .With(x => x.Identifier = "4087738574")
                .TheNext(1)
                .With(x => x.Identifier = "9781455546176")
                .Build()
                .ToList();

            Subject.GetIsbn(ids).Should().Be("9781455546176");
        }

        [Test]
        public void should_read_the_title_and_author_of_a_pdf()
        {
            var info = Subject.ReadTags(GivenPdf(BuildPdf("Some Title", "Some Author")));

            info.BookTitle.Should().Be("Some Title");
            info.Authors.Should().Equal("Some Author");
            info.Quality.Quality.Should().Be(Quality.PDF);
            info.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.TagLib);
        }

        [Test]
        public void should_read_a_pdf_without_document_information_as_empty()
        {
            var info = Subject.ReadTags(GivenPdf(BuildPdf(null, null)));

            info.BookTitle.Should().BeEmpty();
            info.Authors.Should().Equal(string.Empty);
            info.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.TagLib);
        }

        [Test]
        public void should_read_the_quality_from_the_extension_of_a_pdf_that_cannot_be_read()
        {
            var info = Subject.ReadTags(GivenPdf(Encoding.ASCII.GetBytes("Just some junk that is not a pdf at all.")));

            info.Quality.Quality.Should().Be(Quality.PDF);
            info.Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.Extension);

            ExceptionVerification.ExpectedErrors(1);
        }

        private IFileInfo GivenPdf(byte[] contents)
        {
            var path = Path.Combine(TempFolder, "Book.pdf");
            File.WriteAllBytes(path, contents);

            return new FileSystem().FileInfo.FromFileName(path);
        }

        // The smallest well-formed PDF: a single empty page, with a document information dictionary
        // holding the title and author when they are given.
        private static byte[] BuildPdf(string title, string author)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>"
            };

            if (title != null)
            {
                objects.Add($"<< /Title ({title}) /Author ({author}) >>");
            }

            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();

            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(pdf.Length);
                pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            var xref = pdf.Length;
            pdf.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");

            foreach (var offset in offsets)
            {
                pdf.Append($"{offset:D10} 00000 n \n");
            }

            var infoEntry = title != null ? $" /Info {objects.Count} 0 R" : string.Empty;
            pdf.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R{infoEntry} >>\nstartxref\n{xref}\n%%EOF\n");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }
}
