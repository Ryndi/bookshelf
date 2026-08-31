import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearInteractiveImportBookFiles, fetchInteractiveImportBookFiles } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import isAudioQuality from 'Utilities/Quality/isAudioQuality';
import ConfirmImportModalContent from './ConfirmImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.bookFiles'),
    (state, { importedFormats }) => importedFormats,
    (bookFiles, importedFormats) => {
      const {
        items,
        ...otherProps
      } = bookFiles;

      // Importing only replaces files of the same format, so an existing ebook is not at risk
      // from an incoming audiobook and must not be listed as about to be deleted.
      const atRisk = !importedFormats || !importedFormats.length ?
        items :
        items.filter((file) => importedFormats.includes(isAudioQuality(file.quality)));

      return {
        ...otherProps,
        items: atRisk
      };
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportBookFiles,
  clearInteractiveImportBookFiles
};

class ConfirmImportModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      books
    } = this.props;

    this.props.fetchInteractiveImportBookFiles({ bookId: books.map((x) => x.id) });
  }

  componentWillUnmount() {
    this.props.clearInteractiveImportBookFiles();
  }

  //
  // Render

  render() {
    return (
      <ConfirmImportModalContent
        {...this.props}
      />
    );
  }
}

ConfirmImportModalContentConnector.propTypes = {
  books: PropTypes.arrayOf(PropTypes.object).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  importedFormats: PropTypes.arrayOf(PropTypes.bool),
  fetchInteractiveImportBookFiles: PropTypes.func.isRequired,
  clearInteractiveImportBookFiles: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ConfirmImportModalContentConnector);
