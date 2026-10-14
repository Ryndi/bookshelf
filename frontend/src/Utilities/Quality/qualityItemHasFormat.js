import _ from 'lodash';
import { AUDIO_QUALITY_IDS } from './isAudioQuality';

// Takes a quality profile item, which is either a single quality or a group of them. A group
// mixing both formats has a quality of each, so it can be the cutoff for either - the way the
// server judges cutoffs too.
export default function qualityItemHasFormat(item, audio) {
  if (!item) {
    return false;
  }

  const qualities = item.quality ? [item.quality] : _.map(item.items, 'quality');

  return _.some(qualities, (quality) => quality && AUDIO_QUALITY_IDS.includes(quality.id) === audio);
}
