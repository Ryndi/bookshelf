using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.AuthorStats
{
    public interface IAuthorStatisticsService
    {
        List<AuthorStatistics> AuthorStatistics();
        AuthorStatistics AuthorStatistics(int authorId);
    }

    public class AuthorStatisticsService : IAuthorStatisticsService,
        IHandle<AuthorAddedEvent>,
        IHandle<AuthorUpdatedEvent>,
        IHandle<AuthorEditedEvent>,
        IHandle<AuthorBulkEditedEvent>,
        IHandle<AuthorDeletedEvent>,
        IHandle<ModelEvent<QualityProfile>>,
        IHandle<BookAddedEvent>,
        IHandle<BookDeletedEvent>,
        IHandle<BookImportedEvent>,
        IHandle<BookEditedEvent>,
        IHandle<BookUpdatedEvent>,
        IHandle<BookFileDeletedEvent>
    {
        private readonly IAuthorStatisticsRepository _authorStatisticsRepository;
        private readonly ICached<List<BookStatistics>> _cache;

        public AuthorStatisticsService(IAuthorStatisticsRepository authorStatisticsRepository,
                                       ICacheManager cacheManager)
        {
            _authorStatisticsRepository = authorStatisticsRepository;
            _cache = cacheManager.GetCache<List<BookStatistics>>(GetType());
        }

        public List<AuthorStatistics> AuthorStatistics()
        {
            var bookStatistics = _cache.Get("AllAuthors", () => _authorStatisticsRepository.AuthorStatistics());

            return bookStatistics.GroupBy(s => s.AuthorId).Select(s => MapAuthorStatistics(s.ToList())).ToList();
        }

        public AuthorStatistics AuthorStatistics(int authorId)
        {
            var stats = _cache.Get(authorId.ToString(), () => _authorStatisticsRepository.AuthorStatistics(authorId));

            if (stats == null || stats.Count == 0)
            {
                return new AuthorStatistics();
            }

            return MapAuthorStatistics(stats);
        }

        private AuthorStatistics MapAuthorStatistics(List<BookStatistics> bookStatistics)
        {
            var authorStatistics = new AuthorStatistics
            {
                AuthorId = bookStatistics.First().AuthorId,
                BookFileCount = bookStatistics.Sum(s => s.BookFileCount),
                BookCount = bookStatistics.Sum(s => s.BookCount),
                AvailableBookCount = bookStatistics.Sum(s => s.AvailableBookCount),
                TotalBookCount = bookStatistics.Sum(s => s.TotalBookCount),
                EbookCount = bookStatistics.Sum(s => s.EbookCount),
                AvailableEbookCount = bookStatistics.Sum(s => s.AvailableEbookCount),
                AudiobookCount = bookStatistics.Sum(s => s.AudiobookCount),
                AvailableAudiobookCount = bookStatistics.Sum(s => s.AvailableAudiobookCount),
                SizeOnDisk = bookStatistics.Sum(s => s.SizeOnDisk),
                BookStatistics = bookStatistics
            };

            return authorStatistics;
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorAddedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorUpdatedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        // Which formats an author's books want comes from the author's audiobook setting and quality
        // profile, so the counts change with them. The author's own handler sends the updated author
        // to the UI, and runs after this one.
        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorEditedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorBulkEditedEvent message)
        {
            _cache.Remove("AllAuthors");

            foreach (var author in message.Authors)
            {
                _cache.Remove(author.Id.ToString());
            }
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorDeletedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        // Any number of authors may be judged by the profile, so every count is dropped.
        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(ModelEvent<QualityProfile> message)
        {
            _cache.Clear();
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookAddedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookDeletedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookImportedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookEditedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookUpdatedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookFileDeletedEvent message)
        {
            _cache.Remove("AllAuthors");

            var authorId = message.BookFile.Author?.Value?.Id.ToString();
            if (authorId != null)
            {
                _cache.Remove(authorId);
            }
        }
    }
}
