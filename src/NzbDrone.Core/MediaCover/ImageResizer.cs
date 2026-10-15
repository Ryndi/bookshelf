using System;
using System.IO;
using NetVips;
using NLog;
using NzbDrone.Common.Disk;
using VipsImage = NetVips.Image;

namespace NzbDrone.Core.MediaCover
{
    public interface IImageResizer
    {
        void Resize(string source, string destination, int height);
    }

    public class ImageResizer : IImageResizer
    {
        // Saving without metadata needs libvips 8.15. The libvips bundled by NetVips.Native is newer,
        // so this only holds back a system libvips, on a platform with no bundled build.
        internal const int MinimumVersion = (8 << 16) | (15 << 8);

        // libvips' VIPS_MAX_COORD. A thumbnail needs a width to fit in, and this one leaves the height
        // alone to decide the size.
        private const int UnboundedWidth = 10_000_000;

        private static readonly Lazy<string> UnavailableReason = new Lazy<string>(Initialize);

        private readonly IDiskProvider _diskProvider;
        private readonly bool _enabled;

        public ImageResizer(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;

            var reason = UnavailableReason.Value;
            _enabled = reason == null;

            if (!_enabled)
            {
                logger.Info("Cover thumbnails are disabled, full size covers are served instead: {0}. Install libvips 8.15 or later to enable them.", reason);
            }
        }

        public void Resize(string source, string destination, int height)
        {
            if (!_enabled)
            {
                // A thumbnail made before would go on being served for a cover that has changed since.
                if (_diskProvider.FileExists(destination))
                {
                    _diskProvider.DeleteFile(destination);
                }

                return;
            }

            try
            {
                using var image = VipsImage.Thumbnail(source, UnboundedWidth, height: height);

                Save(image, destination);
            }
            catch
            {
                if (_diskProvider.FileExists(destination))
                {
                    _diskProvider.DeleteFile(destination);
                }

                throw;
            }
        }

        private static void Save(VipsImage image, string destination)
        {
            var extension = Path.GetExtension(destination);

            if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                image.Pngsave(destination, keep: Enums.ForeignKeep.None);
            }
            else if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                // Thumbnails don't need super high quality. libvips stops subsampling the colour from
                // Q90 up, which makes thumbnails about a quarter larger for no gain visible at these sizes.
                image.Jpegsave(destination, q: 92, subsampleMode: Enums.ForeignSubsample.On, keep: Enums.ForeignKeep.None);
            }
            else
            {
                // Any other cover keeps the format its name asks for.
                image.WriteToFile(destination);
            }
        }

        internal static string GetUnavailableReason(bool initialized, Exception exception, int? version)
        {
            if (!initialized)
            {
                return $"libvips could not be loaded ({exception?.Message ?? "not found"})";
            }

            if (!version.HasValue || version.Value < MinimumVersion)
            {
                return $"libvips {FormatVersion(version)} is older than 8.15";
            }

            return null;
        }

        private static string FormatVersion(int? version)
        {
            return version.HasValue ? $"{version.Value >> 16}.{(version.Value >> 8) & 0xFF}.{version.Value & 0xFF}" : "of unknown version";
        }

        private static string Initialize()
        {
            var reason = GetUnavailableReason(ModuleInitializer.VipsInitialized, ModuleInitializer.Exception, ModuleInitializer.Version);

            if (reason != null)
            {
                return reason;
            }

            // Covers come from the metadata sources, which serve JPEG, PNG and WebP, so only those are
            // read. Every other format - TIFF among them - is refused before it is decoded.
            Operation.Block("VipsForeignLoad", true);
            Operation.Block("VipsForeignLoadJpeg", false);
            Operation.Block("VipsForeignLoadPng", false);
            Operation.Block("VipsForeignLoadWebp", false);

            // The operation cache holds source files open, which stops a cover being replaced on
            // Windows, and reusing results buys nothing when each cover is resized once.
            Cache.Max = 0;
            Cache.MaxFiles = 0;

            // MediaCoverService already resizes the covers of several authors at once, so one libvips
            // thread for each resize keeps the CPU from being oversubscribed.
            NetVips.NetVips.Concurrency = 1;

            return null;
        }
    }
}
