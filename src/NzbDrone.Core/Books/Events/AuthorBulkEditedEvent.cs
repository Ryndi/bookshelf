using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    // Authors saved together from the author editor. One event for the lot, rather than an
    // AuthorEditedEvent for each, which would set off the work that follows editing one author.
    public class AuthorBulkEditedEvent : IEvent
    {
        public List<Author> Authors { get; private set; }

        public AuthorBulkEditedEvent(List<Author> authors)
        {
            Authors = authors;
        }
    }
}
