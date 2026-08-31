import PropTypes from 'prop-types';
import React from 'react';
import AuthorNameLink from 'Author/AuthorNameLink';
import bookEntities from 'Book/bookEntities';
import BookSearchCellConnector from 'Book/BookSearchCellConnector';
import BookTitleLink from 'Book/BookTitleLink';
import Label from 'Components/Label';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MissingRow.css';

// A book lands here for missing an ebook, or a missing audiobook when it wants one, or both.
// The row otherwise says only that something is missing, not which.
function missingFormats(statistics) {
  const {
    availableEbookCount = 0,
    audiobookCount = 0,
    availableAudiobookCount = 0
  } = statistics || {};

  const formats = [];

  if (!availableEbookCount) {
    formats.push(translate('Ebook'));
  }

  if (audiobookCount > 0 && !availableAudiobookCount) {
    formats.push(translate('Audiobook'));
  }

  return formats;
}

function MissingRow(props) {
  const {
    id,
    author,
    releaseDate,
    titleSlug,
    title,
    lastSearchTime,
    disambiguation,
    statistics,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  if (!author) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {
        columns.map((column) => {
          const {
            name,
            isVisible
          } = column;

          if (!isVisible) {
            return null;
          }

          if (name === 'authorMetadata.sortName') {
            return (
              <TableRowCell key={name}>
                <AuthorNameLink
                  titleSlug={author.titleSlug}
                  authorName={author.authorName}
                />
              </TableRowCell>
            );
          }

          if (name === 'books.title') {
            return (
              <TableRowCell key={name}>
                <BookTitleLink
                  titleSlug={titleSlug}
                  title={title}
                  disambiguation={disambiguation}
                />
              </TableRowCell>
            );
          }

          if (name === 'format') {
            return (
              <TableRowCell key={name}>
                {
                  missingFormats(statistics).map((format) => {
                    return (
                      <Label
                        key={format}
                        className={styles.format}
                        kind={kinds.DANGER}
                      >
                        {format}
                      </Label>
                    );
                  })
                }
              </TableRowCell>
            );
          }

          if (name === 'releaseDate') {
            return (
              <RelativeDateCellConnector
                key={name}
                date={releaseDate}
              />
            );
          }

          if (name === 'books.lastSearchTime') {
            return (
              <RelativeDateCellConnector
                key={name}
                date={lastSearchTime}
              />
            );
          }

          if (name === 'actions') {
            return (
              <BookSearchCellConnector
                key={name}
                bookId={id}
                authorId={author.id}
                bookTitle={title}
                authorName={author.authorName}
                bookEntity={bookEntities.WANTED_MISSING}
                showOpenAuthorButton={true}
              />
            );
          }

          return null;
        })
      }
    </TableRow>
  );
}

MissingRow.propTypes = {
  id: PropTypes.number.isRequired,
  author: PropTypes.object.isRequired,
  releaseDate: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  lastSearchTime: PropTypes.string,
  disambiguation: PropTypes.string,
  statistics: PropTypes.object,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default MissingRow;
