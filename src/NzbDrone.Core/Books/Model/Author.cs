using System;
using System.Collections.Generic;
using Equ;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Books
{
    public class Author : Entity<Author>
    {
        public Author()
        {
            Tags = new HashSet<int>();
            Metadata = new AuthorMetadata();
        }

        // These correspond to columns in the Authors table
        public int AuthorMetadataId { get; set; }
        public string CleanName { get; set; }
        public bool Monitored { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public int QualityProfileId { get; set; }
        public int MetadataProfileId { get; set; }

        // Audiobooks are wanted alongside ebooks rather than instead of them, so they get their
        // own quality profile. Zero means the author is not tracking audiobooks.
        public int AudiobookQualityProfileId { get; set; }
        public bool SearchAudiobooks { get; set; }

        // The root the audiobook folder is built under. AudiobookPath is derived from it the
        // same way Path is derived from RootFolderPath. Empty keeps audiobooks beside the ebooks.
        public string AudiobookRootFolderPath { get; set; }
        public string AudiobookPath { get; set; }
        public HashSet<int> Tags { get; set; }
        [MemberwiseEqualityIgnore]
        public AddAuthorOptions AddOptions { get; set; }

        // Dynamically loaded from DB
        [MemberwiseEqualityIgnore]
        public LazyLoaded<AuthorMetadata> Metadata { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<QualityProfile> AudiobookQualityProfile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<MetadataProfile> MetadataProfile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<Book>> Books { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<Series>> Series { get; set; }

        //compatibility properties
        [MemberwiseEqualityIgnore]
        public string Name
        {
            get { return Metadata.Value.Name; } set { Metadata.Value.Name = value; }
        }

        [MemberwiseEqualityIgnore]
        public string ForeignAuthorId
        {
            get { return Metadata.Value.ForeignAuthorId; } set { Metadata.Value.ForeignAuthorId = value; }
        }

        // Audiobook releases are judged against their own profile when one is set, so an author
        // whose main profile only allows ebook formats can still take audiobooks.
        public QualityProfile ProfileFor(Quality quality)
        {
            if (Quality.IsAudio(quality) && AudiobookQualityProfileId > 0 && AudiobookQualityProfile?.Value != null)
            {
                return AudiobookQualityProfile.Value;
            }

            return QualityProfile.Value;
        }

        public QualityProfile ProfileFor(QualityModel quality)
        {
            return ProfileFor(quality?.Quality);
        }

        // Audiobooks go to their own folder when one is configured, so the two formats can live
        // under different root folders. Falling back to Path keeps existing libraries in place.
        public string PathFor(Quality quality)
        {
            return Quality.IsAudio(quality) && AudiobookPath.IsNotNullOrWhiteSpace() ? AudiobookPath : Path;
        }

        public string PathForExtension(string extension)
        {
            return MediaFileExtensions.IsAudioFile(extension) && AudiobookPath.IsNotNullOrWhiteSpace() ? AudiobookPath : Path;
        }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", Metadata.Value.ForeignAuthorId.NullSafe(), Metadata.Value.Name.NullSafe());
        }

        public override void UseMetadataFrom(Author other)
        {
            CleanName = other.CleanName;
        }

        public override void UseDbFieldsFrom(Author other)
        {
            Id = other.Id;
            AuthorMetadataId = other.AuthorMetadataId;
            Monitored = other.Monitored;
            MonitorNewItems = other.MonitorNewItems;
            LastInfoSync = other.LastInfoSync;
            Path = other.Path;
            RootFolderPath = other.RootFolderPath;
            Added = other.Added;
            QualityProfileId = other.QualityProfileId;
            QualityProfile = other.QualityProfile;
            AudiobookQualityProfileId = other.AudiobookQualityProfileId;
            AudiobookQualityProfile = other.AudiobookQualityProfile;
            SearchAudiobooks = other.SearchAudiobooks;
            AudiobookRootFolderPath = other.AudiobookRootFolderPath;
            AudiobookPath = other.AudiobookPath;
            MetadataProfileId = other.MetadataProfileId;
            MetadataProfile = other.MetadataProfile;
            Tags = other.Tags;
            AddOptions = other.AddOptions;
        }

        public override void ApplyChanges(Author other)
        {
            Path = other.Path;
            QualityProfileId = other.QualityProfileId;
            QualityProfile = other.QualityProfile;
            AudiobookQualityProfileId = other.AudiobookQualityProfileId;
            AudiobookQualityProfile = other.AudiobookQualityProfile;
            SearchAudiobooks = other.SearchAudiobooks;
            AudiobookRootFolderPath = other.AudiobookRootFolderPath;
            AudiobookPath = other.AudiobookPath;
            MetadataProfileId = other.MetadataProfileId;
            MetadataProfile = other.MetadataProfile;

            Books = other.Books;
            Tags = other.Tags;
            AddOptions = other.AddOptions;
            RootFolderPath = other.RootFolderPath;
            Monitored = other.Monitored;
            MonitorNewItems = other.MonitorNewItems;
        }
    }
}
