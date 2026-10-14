using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    // An audiobook is held alongside the ebook rather than in place of it, so nothing that compares
    // it with the files already on disk stops it being grabbed. Whether one is wanted at all is the
    // book's or the author's Search Audiobooks setting, and this is what holds releases to it.
    public class AudiobookWantedSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AudiobookWantedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            if (!Quality.IsAudio(subject.ParsedBookInfo.Quality.Quality))
            {
                return Decision.Accept();
            }

            if (subject.Books.Any(b => subject.Author.WantsAudiobooks(b)))
            {
                return Decision.Accept();
            }

            _logger.Debug("Audiobooks are not wanted for {0}, rejecting", subject);
            return Decision.Reject("Audiobooks are not wanted for this book");
        }
    }
}
