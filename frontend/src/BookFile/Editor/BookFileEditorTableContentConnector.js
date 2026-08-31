/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteBookFile, deleteBookFiles, setBookFilesSort, updateBookFiles } from 'Store/Actions/bookFileActions';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import filterByBookFormat from 'Utilities/Quality/filterByBookFormat';
import getQualities from 'Utilities/Quality/getQualities';
import BookFileEditorTableContent from './BookFileEditorTableContent';

function createSchemaSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const qualities = getQualities(qualityProfiles.schema.items);

      let error = null;

      if (qualityProfiles.schemaError) {
        error = 'Unable to load qualities';
      }

      return {
        isFetching: qualityProfiles.isSchemaFetching,
        isPopulated: qualityProfiles.isSchemaPopulated,
        error,
        qualities
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state, { authorId }) => authorId,
    (state, { bookId }) => bookId,
    (state, { formatFilter }) => formatFilter,
    createClientSideCollectionSelector('bookFiles'),
    createSchemaSelector(),
    (
      authorId,
      bookId,
      formatFilter,
      bookFiles,
      schema
    ) => {
      const {
        items,
        ...otherProps
      } = bookFiles;

      // The store is shared and the server broadcasts every imported file to every client, so
      // an import for an unrelated author lands here too. Without this the file shows up in
      // whichever list happens to be open and only goes away on the next refresh.
      const scoped = items.filter((file) => {
        return bookId == null ? file.authorId === authorId : file.bookId === bookId;
      });

      // Filtered here rather than at render so selection, select-all and delete only ever
      // act on the rows actually on screen.
      return {
        ...schema,
        items: filterByBookFormat(scoped, formatFilter),
        ...otherProps,
        isDeleting: bookFiles.isDeleting,
        isSaving: bookFiles.isSaving
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setBookFilesSort({ sortKey }));
    },

    dispatchFetchQualityProfileSchema(name, path) {
      dispatch(fetchQualityProfileSchema());
    },

    dispatchUpdateBookFiles(updateProps) {
      dispatch(updateBookFiles(updateProps));
    },

    onDeletePress(bookFileIds) {
      dispatch(deleteBookFiles({ bookFileIds }));
    },

    dispatchDeleteBookFile(id) {
      dispatch(deleteBookFile(id));
    }
  };
}

class BookFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Listeners

  onQualityChange = (bookFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateBookFiles({ bookFileIds, quality });
  };

  //
  // Render

  render() {
    const {
      dispatchFetchQualityProfileSchema,
      dispatchUpdateBookFiles,
      ...otherProps
    } = this.props;

    return (
      <BookFileEditorTableContent
        {...otherProps}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

BookFileEditorTableContentConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  bookId: PropTypes.number,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateBookFiles: PropTypes.func.isRequired,
  dispatchDeleteBookFile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(BookFileEditorTableContentConnector);
