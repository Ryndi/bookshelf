import PropTypes from 'prop-types';
import React from 'react';
import BookQuality from 'Book/BookQuality';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import isAudioQuality from 'Utilities/Quality/isAudioQuality';
import translate from 'Utilities/String/translate';
import styles from './BookStatus.css';

function statusFor(bookFile, monitored, isAvailable) {
  if (bookFile) {
    const quality = bookFile.quality;

    return (
      <BookQuality
        title={quality.quality.name}
        size={bookFile.size}
        quality={quality}
        isMonitored={monitored}
        isCutoffNotMet={bookFile.qualityCutoffNotMet}
      />
    );
  }

  if (!monitored) {
    return (
      <Label
        title={translate('NotMonitored')}
        kind={kinds.WARNING}
      >
        {translate('NotMonitored')}
      </Label>
    );
  }

  if (isAvailable) {
    return (
      <Label
        title={translate('BookAvailableButMissing')}
        kind={kinds.DANGER}
      >
        {translate('Missing')}
      </Label>
    );
  }

  return (
    <Label
      title={translate('NotAvailable')}
      kind={kinds.INFO}
    >
      {translate('NotAvailable')}
    </Label>
  );
}

function BookStatus(props) {
  const {
    isAvailable,
    monitored,
    bookFiles,
    wantsAudiobooks
  } = props;

  // Only one format is being tracked, so naming it would be noise.
  if (!wantsAudiobooks) {
    return (
      <div className={styles.center}>
        {statusFor(bookFiles[0], monitored, isAvailable)}
      </div>
    );
  }

  // Both are tracked, so each one needs its own status - a single label cannot say that the
  // ebook is here and the audiobook is still missing.
  const formats = [
    {
      key: 'ebook',
      label: translate('Ebook'),
      bookFile: bookFiles.find((file) => !isAudioQuality(file.quality))
    },
    {
      key: 'audiobook',
      label: translate('Audiobook'),
      bookFile: bookFiles.find((file) => isAudioQuality(file.quality))
    }
  ];

  return (
    <div className={styles.formats}>
      {
        formats.map(({ key, label, bookFile }) => {
          return (
            <div
              key={key}
              className={styles.format}
            >
              <span className={styles.formatLabel}>
                {label}
              </span>

              {statusFor(bookFile, monitored, isAvailable)}
            </div>
          );
        })
      }
    </div>
  );
}

BookStatus.propTypes = {
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool.isRequired,
  bookFiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  wantsAudiobooks: PropTypes.bool
};

BookStatus.defaultProps = {
  bookFiles: [],
  wantsAudiobooks: false
};

export default BookStatus;
