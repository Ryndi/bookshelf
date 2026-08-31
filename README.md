# bookshelf

A fork of [klein1jo/bookshelf](https://github.com/klein1jo/bookshelf) that adds support for
keeping **both an ebook and an audiobook** of the same book, rather than having to choose one.

Upstream, a book can only ever track a single edition, and acquiring one format replaces the
other on disk. This fork changes that.

## Lineage

    Readarr/Readarr              abandoned
      └─ pennydreadful/bookshelf abandoned
           └─ klein1jo/bookshelf merges open Bookshelf pull requests
                └─ this fork     adds ebook + audiobook support

Everything klein1jo merges is carried here — the Hardcover import, the qBittorrent auth fixes,
the .NET Windows installer and the rest. Their updates are pulled in periodically.

## Ebook and audiobook side by side

- A book keeps **one monitored edition per format**, so an ebook and an audiobook can be tracked
  at the same time.
- Quality is judged **per format**, with its own cutoff for each. Owning an audiobook no longer
  makes every ebook look like a downgrade.
- A book stays **wanted** while either format is missing, so acquiring the ebook does not stop
  the audiobook being searched for.
- Importing one format **never replaces** the other on disk.
- Searches fan out per monitored edition, so an audiobook titled differently to the ebook is
  still found.

### Setting it up

Per author, under **Edit**:

| Setting | |
|---|---|
| **Search Audiobooks** | Turn it on to want an audiobook alongside the ebook |
| **Audiobook Quality Profile** | Which profile judges audiobook releases — typically one allowing MP3/M4B/FLAC |
| **Audiobook Root Folder** | Where audiobooks are stored. The author folder is created under it using your usual naming |

The same three settings are available when adding an author and in the bulk author editor. Each
book has a **Search Audiobooks** override that inherits from its author unless you set it.

Quality profiles gain a separate **Upgrade Audiobook Until** cutoff alongside the ebook one, and
the History, Search and Files tabs on a book have an **All / Ebook / Audiobook** filter.

## Getting started

Drop-in replacement for an existing Bookshelf or klein1jo/bookshelf install.

Goodreads-derived metadata:

    docker run -p 8787:8787 -v ~/.config/bookshelf:/config ghcr.io/ryndi/bookshelf:softcover

[Hardcover](https://hardcover.app/home) metadata:

    docker run -p 8787:8787 -v ~/.config/bookshelf:/config ghcr.io/ryndi/bookshelf:hardcover

### Back up your config first

This applies database migrations **41 through 44**, and they are one-way — there is no
downgrade path. Once they have run, going back to another build means restoring your config
directory from a backup.

    cp -a ~/.config/bookshelf ~/.config/bookshelf-backup

## Support

None offered, and this is not affiliated with Readarr, Bookshelf or klein1jo. Issues are open
for problems with the audiobook support specifically.

Anything wrong with the underlying application belongs upstream at
[Bookshelf](https://github.com/pennydreadful/bookshelf), so it is captured where it originated.
