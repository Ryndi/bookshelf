using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAuthorService _authorService;

        public MountCheck(IDiskProvider diskProvider, IAuthorService authorService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _authorService = authorService;
        }

        public override HealthCheck Check()
        {
            // Audiobooks can be kept under a root of their own, which may be mounted read-only too.
            var audiobookPaths = _authorService.GetAllAuthors()
                                               .Select(a => a.AudiobookPath)
                                               .Where(p => p.IsNotNullOrWhiteSpace());

            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _authorService.AllAuthorPaths()
                                      .Select(p => p.Value)
                                      .Concat(audiobookPaths)
                                      .Select(p => _diskProvider.GetMount(p))
                                      .Where(m => m != null && m.MountOptions != null && m.MountOptions.IsReadOnly)
                                      .DistinctBy(m => m.RootDirectory)
                                      .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("MountCheckMessage") + string.Join(", ", mounts.Select(m => m.Name)), "#author-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
