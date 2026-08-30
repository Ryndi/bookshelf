import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import SelectInput from './SelectInput';

const NONE = '';

function editionLabel(bookEdition) {
  let label = `${bookEdition.title}`;

  if (bookEdition.disambiguation) {
    label = `${label} (${titleCase(bookEdition.disambiguation)})`;
  }

  const extras = [];
  if (bookEdition.language) {
    extras.push(bookEdition.language);
  }
  if (bookEdition.publisher) {
    extras.push(bookEdition.publisher);
  }
  if (bookEdition.isbn13) {
    extras.push(bookEdition.isbn13);
  }
  if (bookEdition.format) {
    extras.push(bookEdition.format);
  }
  if (bookEdition.pageCount > 0) {
    extras.push(`${bookEdition.pageCount}p`);
  }

  if (extras.length) {
    label = `${label} [${extras.join(', ')}]`;
  }

  return label;
}

function createMapStateToProps() {
  return createSelector(
    (state, { bookEditions }) => bookEditions,
    (state, { isAudiobook }) => isAudiobook,
    (bookEditions, isAudiobook) => {
      const all = bookEditions.value || [];
      const inGroup = _.filter(all, (e) => Boolean(e.isAudiobook) === isAudiobook);
      const others = _.filter(all, (e) => Boolean(e.isAudiobook) !== isAudiobook);

      const values = _.orderBy(
        _.map(inGroup, (bookEdition) => ({
          key: bookEdition.foreignEditionId,
          value: editionLabel(bookEdition)
        })),
        ['value']
      );

      // Only offer "none" when the other format is tracking something, so the book
      // can never be left with no monitored edition at all.
      if (_.some(others, { monitored: true })) {
        values.unshift({ key: NONE, value: translate('None') });
      }

      const monitored = _.find(inGroup, { monitored: true });

      return {
        values,
        value: monitored ? monitored.foreignEditionId : NONE
      };
    }
  );
}

class BookEditionSelectInputConnector extends Component {

  //
  // Listeners

  onChange = ({ name, value }) => {
    const {
      bookEditions,
      isAudiobook
    } = this.props;

    // Editions of the other format keep whatever they had - a book monitors one per format.
    const updatedEditions = _.map(bookEditions.value, (e) => {
      if (Boolean(e.isAudiobook) !== isAudiobook) {
        return e;
      }

      return { ...e, monitored: e.foreignEditionId === value };
    });

    this.props.onChange({ name, value: updatedEditions });
  };

  render() {
    const {
      isAudiobook,
      ...otherProps
    } = this.props;

    return (
      <SelectInput
        {...otherProps}
        onChange={this.onChange}
      />
    );
  }
}

BookEditionSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  bookEditions: PropTypes.object,
  isAudiobook: PropTypes.bool
};

BookEditionSelectInputConnector.defaultProps = {
  isAudiobook: false
};

export default connect(createMapStateToProps)(BookEditionSelectInputConnector);
