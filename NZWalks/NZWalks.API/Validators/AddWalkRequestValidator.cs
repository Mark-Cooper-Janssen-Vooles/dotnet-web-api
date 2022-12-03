using FluentValidation;

namespace NZWalks.API.Validators
{
    public class AddWalkRequestValidator : AbstractValidator<Models.DTO.AddWalkRequest>
    {
        public AddWalkRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Length).GreaterThan(0);
            // check if regionId exists
            // check if walkDifficultyId exists
        }
    }
}
