using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.BookTests
{
    [TestFixture]
    public class BookFormatFixture : CoreTest
    {
        // Values seen coming back from the metadata API.
        [TestCase("Audiobook")]
        [TestCase("Audible Audio")]
        [TestCase("Audio CD")]
        [TestCase("Audio Cassette")]
        [TestCase("MP3 CD")]
        public void should_recognise_known_audiobook_formats(string format)
        {
            BookFormat.IsAudiobook(format).Should().BeTrue();
        }

        // Casing is inconsistent in the metadata - "Paperback" and "paperback" both occur.
        [TestCase("audiobook")]
        [TestCase("AUDIOBOOK")]
        [TestCase("audible audio")]
        public void should_ignore_casing(string format)
        {
            BookFormat.IsAudiobook(format).Should().BeTrue();
        }

        [TestCase("Digital Audiobook")]
        [TestCase("Audiobook (Unabridged)")]
        [TestCase("Audio Book")]
        [TestCase("Cassette")]
        public void should_match_audiobook_wordings_outside_the_fixed_list(string format)
        {
            BookFormat.IsAudiobook(format).Should().BeTrue();
        }

        [TestCase("Paperback")]
        [TestCase("paperback")]
        [TestCase("Hardcover")]
        [TestCase("Mass Market Paperback")]
        [TestCase("Paperbak")]
        [TestCase("Kindle Edition")]
        [TestCase("ebook")]
        [TestCase("ePub")]
        [TestCase("")]
        [TestCase(null)]
        public void should_not_treat_print_or_ebook_formats_as_audiobooks(string format)
        {
            BookFormat.IsAudiobook(format).Should().BeFalse();
        }

        [TestCase("ebook")]
        [TestCase("Kindle Edition")]
        [TestCase("ePub")]
        [TestCase("EPUB")]
        [TestCase("Nook")]
        public void should_recognise_ebook_formats(string format)
        {
            BookFormat.IsEbook(format).Should().BeTrue();
        }

        [TestCase("Audiobook")]
        [TestCase("Paperback")]
        [TestCase(null)]
        public void should_not_treat_other_formats_as_ebooks(string format)
        {
            BookFormat.IsEbook(format).Should().BeFalse();
        }
    }
}
