import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearEditions, fetchEditions } from 'Store/Actions/editionActions';
import {
  saveInteractiveImportItem,
  updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import SelectEditionModalContent from './SelectEditionModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.editions,
    (editions) => {
      const {
        isFetching,
        isPopulated,
        error
      } = editions;

      return {
        isFetching,
        isPopulated,
        error
      };
    }
  );
}

const mapDispatchToProps = {
  fetchEditions,
  clearEditions,
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectEditionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.populate);
    this.unpopulate();
  }
  //
  // Control

  populate = () => {
    // A book being imported in both formats has a row for each.
    const bookId = _.uniq(this.props.books.map((b) => b.book.id));

    this.props.fetchEditions({ bookId });
  };

  unpopulate = () => {
    this.props.clearEditions();
  };

  //
  // Listeners

  onEditionSelect = (key, foreignEditionId) => {
    // Only the files of this row's book and format move to the chosen edition.
    const { ids } = _.find(this.props.books, { key });

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        foreignEditionId,
        disableReleaseSwitching: true,
        tracks: [],
        rejections: []
      });
    });

    this.props.saveInteractiveImportItem({ ids });

    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <SelectEditionModalContent
        {...this.props}
        onEditionSelect={this.onEditionSelect}
      />
    );
  }
}

SelectEditionModalContentConnector.propTypes = {
  books: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchEditions: PropTypes.func.isRequired,
  clearEditions: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectEditionModalContentConnector);
