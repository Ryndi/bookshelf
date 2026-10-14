import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormInputGroup from 'Components/Form/FormInputGroup';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { inputTypes } from 'Helpers/Props';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

class SelectEditionRow extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.onEditionSelect(name, value);
  };

  //
  // Render

  render() {
    const {
      groupKey,
      isAudiobook,
      showFormat,
      matchedEditionId,
      title,
      disambiguation,
      editions,
      columns
    } = this.props;

    let extendedTitle = disambiguation ? `${title} (${disambiguation})` : title;

    if (showFormat) {
      extendedTitle = `${extendedTitle} - ${translate(isAudiobook ? 'Audiobook' : 'Ebook')}`;
    }

    const options = _.map(editions, (bookEdition) => {

      let value = `${bookEdition.title}`;

      if (bookEdition.disambiguation) {
        value = `${value} (${titleCase(bookEdition.disambiguation)})`;
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
      if (bookEdition.asin) {
        extras.push(bookEdition.asin);
      }
      if (bookEdition.format) {
        extras.push(bookEdition.format);
      }
      if (bookEdition.pageCount > 0) {
        extras.push(`${bookEdition.pageCount}p`);
      }

      if (extras.length) {
        value = `${value} [${extras.join(', ')}]`;
      }

      return {
        key: bookEdition.foreignEditionId,
        value,
        isOwnFormat: Boolean(bookEdition.isAudiobook) === isAudiobook
      };
    });

    // Editions of the row's own format come first. The rest stay on offer, since an edition's
    // format is free text in the metadata and is not always filled in.
    const sortedValues = _.orderBy(options, ['isOwnFormat', 'value'], ['desc', 'asc'])
      .map(({ key, value }) => ({ key, value }));

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'book') {
              return (
                <TableRowCell key={name}>
                  {extendedTitle}
                </TableRowCell>
              );
            }

            if (name === 'edition') {
              return (
                <TableRowCell key={name}>
                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name={groupKey}
                    values={sortedValues}
                    value={matchedEditionId}
                    onChange={this.onInputChange}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }
      </TableRow>

    );
  }
}

SelectEditionRow.propTypes = {
  groupKey: PropTypes.string.isRequired,
  isAudiobook: PropTypes.bool.isRequired,
  showFormat: PropTypes.bool.isRequired,
  matchedEditionId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  editions: PropTypes.arrayOf(PropTypes.object).isRequired,
  onEditionSelect: PropTypes.func.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SelectEditionRow;
