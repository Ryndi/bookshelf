// Mirrors Quality.AudioIds in src/NzbDrone.Core/Qualities/Quality.cs. Quality ids are persisted
// in the database, so they are fixed.
export const AUDIO_QUALITY_IDS = [10, 11, 12, 13];

// Takes a QualityModel as it appears on a history record, release or book file.
export default function isAudioQuality(qualityModel) {
  const id = qualityModel && qualityModel.quality ? qualityModel.quality.id : null;

  return AUDIO_QUALITY_IDS.includes(id);
}
