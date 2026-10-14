using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Validation
{
    // A profile picked for audiobooks has to take an audio quality. Otherwise every audiobook release
    // is turned away as unwanted, and the books stay on the Missing list for good.
    public class AudiobookQualityProfileValidator : PropertyValidator
    {
        private readonly IQualityProfileService _qualityProfileService;

        public AudiobookQualityProfileValidator(IQualityProfileService qualityProfileService)
        {
            _qualityProfileService = qualityProfileService;
        }

        protected override string GetDefaultMessageTemplate() => "Quality Profile takes no audiobook quality";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context?.PropertyValue == null || (int)context.PropertyValue == 0)
            {
                return true;
            }

            var id = (int)context.PropertyValue;

            // A profile that does not exist is reported by QualityProfileExistsValidator.
            return !_qualityProfileService.Exists(id) || _qualityProfileService.Get(id).Allows(true);
        }
    }
}
