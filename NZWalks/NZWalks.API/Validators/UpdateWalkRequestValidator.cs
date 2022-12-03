using FluentValidation;

namespace NZWalks.API.Validators
{
    public class UpdateWalkRequestValidator :AbstractValidator<Models.DTO.UpdateWalkRequest>
    {
        public UpdateWalkRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Length).GreaterThan(0);
            // check if regionId exists. It is possible to use fluent validator to do this, but its complex.
            // check if walkDifficultyId exists. It is possible to use fluent validator to do this, but its complex.

            // preference above is to not use fluent validator to check db
        }
    }
}
