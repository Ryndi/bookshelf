# bookshelf

This fork merges [Bookshelf pull requests](https://github.com/pennydreadful/bookshelf/pulls)
as Bookshelf has seemingly been abandoned.  As long as Bookshelf remains inactive this fork
will continue to merge new pull requests.  If Bookshelf is ever updated and remains active
this fork will defer to Bookshelf.

This repository is forked from Bookshelf commit c21c413, which was the latest at the time,
and merges the following open pull requests:

    - #171 Hardcover import from any username and reading status
    - #163 feat(QBittorrent): port API-key auth from Radarr/Sonarr
    - #162 fix(QBittorrentProxyV2): accept empty body as auth success (qBit >= 4.5)
    - #161 fix(AudioTag): null-safe Diff() for OriginalReleaseDate
    - #159 Corrected issue with updating book edition. (#96)
    - #154 Add .NET Windows installer with safe upgrade and auto-rollback
    - #151 Paginate large Goodreads series
    - #149 Cleanse database connection string when logging
    - #119 Update name_map.json

Open pull requests not merged:

    - #170 - Duplicate of pull request #162 - Fix qBittorrent 5.2.0 authentication failure (HTTP 204 on login)
    - #158 - Duplicate of pull request #162 - Fix qBittorrent V2 auth check breaking against qBit 5.2.0+ (empty 204 response)
    - #132 - Failed status checks - Add configurable UI setting for import match threshold
    - #128 - Integrated by pull request #171 - Add Hardcover reading status import support
    - #123 - Integrated by request #171 - Change Hardcover Lists to be from any user by username

## Getting Started

This fork is a direct drop-in replacement for your current Bookshelf installation.  The following are
provided as examples only and may differ from your installation.

If you were previously using [Goodreads](https://www.goodreads.com) as your metadata provider:

    docker run -p 8787:8787 -v ~/.config/bookshelf:/config ghcr.io/klein1jo/bookshelf:softcover

If you were previously using [Hardcover](https://hardcover.app/home) as a metadata provider:

    docker run -p 8787:8787 -v ~/.config/bookshelf:/config ghcr.io/klein1jo/bookshelf:hardcover

## Support

I offer no support.  Issues and pull requests have been disabled on this repository.  If you have
an issue or wish to submit a pull request please do so to [Bookshelf](https://github.com/pennydreadful/bookshelf)
so that they are captured in the upstream.

