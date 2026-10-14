import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllAuthorsSelector(),
    (state) => state.settings.importLists.items,
    (id, author, lists) => {
      if (!id) {
        return false;
      }

      // An author can use a quality profile for its audiobooks as well as its main one.
      const authorProps = profileProp === 'qualityProfileId' ? [profileProp, 'audiobookQualityProfileId'] : [profileProp];

      if (_.some(author, (a) => authorProps.some((prop) => a[prop] === id)) || _.some(lists, { [profileProp]: id })) {
        return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
