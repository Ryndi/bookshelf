import PropTypes from 'prop-types';
import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Author/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './BookIndexProgressBar.css';

function BookIndexProgressBar(props) {
  const {
    monitored,
    bookCount,
    bookFileCount,
    totalBookCount,
    ebookCount,
    availableEbookCount,
    audiobookCount,
    availableAudiobookCount,
    posterWidth,
    detailedProgressBar
  } = props;

  // A book wanting both formats is complete only once it holds both. Counted by files alone,
  // holding the ebook would show it as done with the audiobook still missing.
  if (ebookCount && audiobookCount) {
    const ebooks = availableEbookCount ? 1 : 0;
    const audiobooks = availableAudiobookCount ? 1 : 0;
    const formatProgress = ((ebooks + audiobooks) / 2) * 100;

    return (
      <ProgressBar
        className={styles.progressBar}
        containerClassName={styles.progress}
        progress={100}
        kind={getProgressBarKind('ended', monitored, formatProgress)}
        size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
        showText={detailedProgressBar}
        text={`${ebooks + audiobooks} / 2`}
        title={`${translate('Ebook')}: ${ebooks} / 1, ${translate('Audiobook')}: ${audiobooks} / 1`}
        width={posterWidth}
      />
    );
  }

  const progress = bookFileCount && bookCount ? (totalBookCount / bookCount) * 100 : 0;
  const text = `${bookFileCount ? bookCount : 0} / ${totalBookCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={100}
      kind={getProgressBarKind('ended', monitored, progress)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={translate('BookProgressBarText', {
        bookCount: bookFileCount ? bookCount : 0,
        bookFileCount,
        totalBookCount
      })}
      width={posterWidth}
    />
  );
}

BookIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  bookCount: PropTypes.number.isRequired,
  bookFileCount: PropTypes.number.isRequired,
  totalBookCount: PropTypes.number.isRequired,
  ebookCount: PropTypes.number,
  availableEbookCount: PropTypes.number,
  audiobookCount: PropTypes.number,
  availableAudiobookCount: PropTypes.number,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

BookIndexProgressBar.defaultProps = {
  ebookCount: 0,
  availableEbookCount: 0,
  audiobookCount: 0,
  availableAudiobookCount: 0
};

export default BookIndexProgressBar;
