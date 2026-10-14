import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByName from 'Utilities/Array/sortByName';
import isAudioQualityItem from 'Utilities/Quality/isAudioQualityItem';
import translate from 'Utilities/String/translate';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.qualityProfiles', sortByName),
    (state, { includeNoChange }) => includeNoChange,
    (state, { includeNoChangeDisabled }) => includeNoChangeDisabled,
    (state, { includeMixed }) => includeMixed,
    (qualityProfiles, includeNoChange, includeNoChangeDisabled = true, includeMixed) => {
      const values = _.map(qualityProfiles.items, (qualityProfile) => {
        return {
          key: qualityProfile.id,
          value: qualityProfile.name
        };
      });

      if (includeNoChange) {
        values.unshift({
          key: 'noChange',
          value: translate('NoChange'),
          isDisabled: includeNoChangeDisabled
        });
      }

      if (includeMixed) {
        values.unshift({
          key: 'mixed',
          value: '(Mixed)',
          isDisabled: true
        });
      }

      // Profiles that take an audiobook quality, so a select for an audiobook profile can start on one.
      const audioProfileIds = qualityProfiles.items
        .filter((qualityProfile) => qualityProfile.items.some((item) => item.allowed && isAudioQualityItem(item)))
        .map((qualityProfile) => qualityProfile.id);

      return {
        values,
        audioProfileIds
      };
    }
  );
}

class QualityProfileSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      name,
      value,
      values,
      audioProfileIds,
      preferAudio
    } = this.props;

    if (!value || !values.some((option) => option.key === value || parseInt(option.key) === value)) {
      const profiles = values.filter((option) => !isNaN(parseInt(option.key)));

      // An audiobook profile starts on one that takes audio. The first by name is usually
      // "eBook", which would turn every audiobook away without anyone having chosen it.
      const firstValue = (preferAudio && profiles.find((option) => audioProfileIds.includes(option.key))) || profiles[0];

      if (firstValue) {
        this.onChange({ name, value: firstValue.key });
      }
    }
  }

  //
  // Listeners

  onChange = ({ name, value }) => {
    this.props.onChange({ name, value: value === 'noChange' ? value : parseInt(value) });
  };

  //
  // Render

  render() {
    return (
      <SelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

QualityProfileSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  audioProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  includeNoChange: PropTypes.bool.isRequired,
  preferAudio: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

QualityProfileSelectInputConnector.defaultProps = {
  includeNoChange: false,
  preferAudio: false
};

export default connect(createMapStateToProps)(QualityProfileSelectInputConnector);
