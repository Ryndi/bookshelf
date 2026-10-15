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
    ebookCount,
    availableEbookCount,
    audiobookCount,
    availableAudiobookCount,
    posterWidth,
    detailedProgressBar
  } = props;

  // While the books want one format only, a single bar still says everything there is to say.
  // An audiobook library - a profile taking no ebook at all - counts its audiobooks on it.
  if (!audiobookCount || !ebookCount) {
    const audiobooksOnly = audiobookCount > 0;
    const available = audiobooksOnly ? availableAudiobookCount : availableBookCount;
    const total = audiobooksOnly ? audiobookCount : bookCount;
    const progress = total ? (available / total) * 100 : 100;

    return (
      <ProgressBar
        className={styles.progressBar}
        containerClassName={styles.progress}
        progress={progress}
        kind={getProgressBarKind(status, monitored, progress)}
        size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
        showText={detailedProgressBar}
        text={`${available} / ${total}`}
        title={translate('AuthorProgressBarText', { bookCount: total, availableBookCount: available, bookFileCount, totalBookCount })}
        width={posterWidth}
      />
    );
  }

  // Each format gets its own bar. A combined one would read as complete while a whole format
  // is still missing. Each total is its own, since only some books may want an audiobook.
  const formats = [
    {
      key: 'ebook',
      label: translate('Ebook'),
      available: availableEbookCount,
      total: ebookCount
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
  ebookCount: PropTypes.number,
  availableEbookCount: PropTypes.number,
  audiobookCount: PropTypes.number,
  availableAudiobookCount: PropTypes.number,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

AuthorIndexProgressBar.defaultProps = {
  ebookCount: 0,
  availableEbookCount: 0,
  audiobookCount: 0,
  availableAudiobookCount: 0
};

export default AuthorIndexProgressBar;
