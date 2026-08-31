import isAudioQuality from './isAudioQuality';

// Filters rows that carry a QualityModel - history records, releases, book files - down to a
// single format. Anything without a quality is left in, so nothing disappears unexplained.
export default function filterByBookFormat(items, format) {
  if (!items || !format || format === 'all') {
    return items;
  }

  const wantAudio = format === 'audiobook';

  return items.filter((item) => {
    if (!item || !item.quality) {
      return true;
    }

    return isAudioQuality(item.quality) === wantAudio;
  });
}
