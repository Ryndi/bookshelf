
function getNewAuthor(author, payload) {
  const {
    rootFolderPath,
    monitor,
    monitorNewItems,
    qualityProfileId,
    searchAudiobooks = false,
    audiobookQualityProfileId = 0,
    audiobookRootFolderPath = '',
    metadataProfileId,
    tags,
    searchForMissingBooks = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingBooks
  };

  author.addOptions = addOptions;
  author.monitored = true;
  author.monitorNewItems = monitorNewItems;
  author.qualityProfileId = qualityProfileId;
  author.searchAudiobooks = searchAudiobooks;
  author.audiobookQualityProfileId = audiobookQualityProfileId;
  author.audiobookRootFolderPath = audiobookRootFolderPath;
  author.metadataProfileId = metadataProfileId;
  author.rootFolderPath = rootFolderPath;
  author.tags = tags;

  return author;
}

export default getNewAuthor;
