import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addBook, setBookAddDefault } from 'Store/Actions/searchActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewBookModalContent from './AddNewBookModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { isExistingAuthor }) => isExistingAuthor,
    (state) => state.search,
    (state) => state.settings.metadataProfiles,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (isExistingAuthor, searchState, metadataProfiles, dimensions, systemStatus) => {
      const {
        isAdding,
        addError,
        bookDefaults
      } = searchState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(bookDefaults, {}, addError);

      return {
        isAdding,
        addError,
        showMetadataProfile: true,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        isWindows: systemStatus.isWindows,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setBookAddDefault,
  addBook
};

class AddNewBookModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setBookAddDefault({ [name]: value });
  };

  onAddBookPress = (searchForNewBook) => {
    const {
      foreignBookId,
      rootFolderPath,
      monitor,
      monitorNewItems,
      qualityProfileId,
      searchAudiobooks,
      audiobookQualityProfileId,
      audiobookRootFolderPath,
      metadataProfileId,
      tags
    } = this.props;

    // The dialog offers the audiobook settings for an author being added with the book, and they
    // are only sent when the toggle is on - the same as adding the author on their own.
    const wantsAudiobooks = Boolean(searchAudiobooks && searchAudiobooks.value);

    this.props.addBook({
      foreignBookId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      monitorNewItems: monitorNewItems.value,
      qualityProfileId: qualityProfileId.value,
      searchAudiobooks: wantsAudiobooks,
      audiobookQualityProfileId: wantsAudiobooks && audiobookQualityProfileId ? audiobookQualityProfileId.value : 0,
      audiobookRootFolderPath: wantsAudiobooks && audiobookRootFolderPath ? audiobookRootFolderPath.value : '',
      metadataProfileId: metadataProfileId.value,
      tags: tags.value,
      searchForNewBook
    });
  };

  //
  // Render

  render() {
    return (
      <AddNewBookModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddBookPress={this.onAddBookPress}
      />
    );
  }
}

AddNewBookModalContentConnector.propTypes = {
  isExistingAuthor: PropTypes.bool.isRequired,
  foreignBookId: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  monitorNewItems: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  searchAudiobooks: PropTypes.object,
  audiobookQualityProfileId: PropTypes.object,
  audiobookRootFolderPath: PropTypes.object,
  metadataProfileId: PropTypes.object,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setBookAddDefault: PropTypes.func.isRequired,
  addBook: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewBookModalContentConnector);
