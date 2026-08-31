import PropTypes from 'prop-types';
import React, { Component } from 'react';
import translate from 'Utilities/String/translate';
import styles from './BookFormatFilter.css';

export const ALL = 'all';
export const EBOOK = 'ebook';
export const AUDIOBOOK = 'audiobook';

class BookFormatFilter extends Component {

  //
  // Render

  render() {
    const {
      value,
      onChange
    } = this.props;

    const options = [
      { key: ALL, label: translate('All') },
      { key: EBOOK, label: translate('Ebook') },
      { key: AUDIOBOOK, label: translate('Audiobook') }
    ];

    return (
      <div className={styles.formatFilter}>
        {
          options.map((option) => {
            return (
              <span
                key={option.key}
                className={option.key === value ? styles.selectedOption : styles.option}
                role="button"
                tabIndex={0}
                onClick={() => onChange(option.key)}
                onKeyDown={(event) => {
                  if (event.key === 'Enter' || event.key === ' ') {
                    onChange(option.key);
                  }
                }}
              >
                {option.label}
              </span>
            );
          })
        }
      </div>
    );
  }
}

BookFormatFilter.propTypes = {
  value: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired
};

export default BookFormatFilter;
