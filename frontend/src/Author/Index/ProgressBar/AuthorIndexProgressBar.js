import PropTypes from 'prop-types';
import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Author/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './AuthorIndexProgressBar.css';

function AuthorIndexProgressBar(props) {
  const {
    monitored,
    status,
    bookCount,
    availableBookCount,
    bookFileCount,
    totalBookCount,
    availableEbookCount,
    audiobookCount,
    availableAudiobookCount,
    posterWidth,
    detailedProgressBar
  } = props;

  // No book here wants an audiobook, so a single bar still says everything there is to say.
  if (!audiobookCount) {
    const progress = bookCount ? (availableBookCount / bookCount) * 100 : 100;
    const text = `${availableBookCount} / ${bookCount}`;

    return (
      <ProgressBar
        className={styles.progressBar}
        containerClassName={styles.progress}
        progress={progress}
        kind={getProgressBarKind(status, monitored, progress)}
        size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
        showText={detailedProgressBar}
        text={text}
        title={translate('AuthorProgressBarText', { bookCount, availableBookCount, bookFileCount, totalBookCount })}
        width={posterWidth}
      />
    );
  }

  // Each format gets its own bar. A combined one would read as complete while a whole format
  // is still missing. The audiobook total is its own, since only some books may want one.
  const formats = [
    {
      key: 'ebook',
      label: translate('Ebook'),
      available: availableEbookCount,
      total: bookCount
    },
    {
      key: 'audiobook',
      label: translate('Audiobook'),
      available: availableAudiobookCount,
      total: audiobookCount
    }
  ];

  return (
    <div className={styles.formats}>
      {
        formats.map(({ key, label, available, total }) => {
          const progress = total ? (available / total) * 100 : 100;

          return (
            <ProgressBar
              key={key}
              className={styles.progressBar}
              containerClassName={styles.progress}
              progress={progress}
              kind={getProgressBarKind(status, monitored, progress)}
              size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
              showText={detailedProgressBar}
              text={`${label} ${available} / ${total}`}
              title={`${label}: ${available} / ${total}`}
              width={posterWidth}
            />
          );
        })
      }
    </div>
  );
}

AuthorIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  bookCount: PropTypes.number.isRequired,
  availableBookCount: PropTypes.number.isRequired,
  bookFileCount: PropTypes.number.isRequired,
  totalBookCount: PropTypes.number.isRequired,
  availableEbookCount: PropTypes.number,
  audiobookCount: PropTypes.number,
  availableAudiobookCount: PropTypes.number,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

AuthorIndexProgressBar.defaultProps = {
  availableEbookCount: 0,
  audiobookCount: 0,
  availableAudiobookCount: 0
};

export default AuthorIndexProgressBar;
