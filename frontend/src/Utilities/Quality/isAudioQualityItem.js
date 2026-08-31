import _ from 'lodash';
import { AUDIO_QUALITY_IDS } from './isAudioQuality';

// Takes a quality profile item, which is either a single quality or a group of them.
// A group counts as audio if any member quality is.
export default function isAudioQualityItem(item) {
  if (!item) {
    return false;
  }

  if (item.quality) {
    return AUDIO_QUALITY_IDS.includes(item.quality.id);
  }

  return _.some(item.items, (i) => i.quality && AUDIO_QUALITY_IDS.includes(i.quality.id));
}
