import _ from 'lodash';

// Mirrors Quality.AudioIds in src/NzbDrone.Core/Qualities/Quality.cs. Quality ids are persisted
// in the database, so they are fixed. A group counts as audio if any member quality is.
const AUDIO_QUALITY_IDS = [10, 11, 12, 13];

export default function isAudioQualityItem(item) {
  if (!item) {
    return false;
  }

  if (item.quality) {
    return AUDIO_QUALITY_IDS.includes(item.quality.id);
  }

  return _.some(item.items, (i) => i.quality && AUDIO_QUALITY_IDS.includes(i.quality.id));
}
