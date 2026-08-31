import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchConnector from './InteractiveSearchConnector';

function InteractiveSearchTable(props) {
  const {
    type,
    formatFilter,
    ...otherProps
  } = props;

  // formatFilter only narrows what is displayed, so it must stay out of the payload sent
  // to the indexers.
  return (
    <InteractiveSearchConnector
      searchPayload={otherProps}
      type={type}
      formatFilter={formatFilter}
    />
  );
}

InteractiveSearchTable.propTypes = {
  type: PropTypes.string.isRequired,
  formatFilter: PropTypes.string
};

export default InteractiveSearchTable;
