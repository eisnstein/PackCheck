using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Validation
{
    public class ValidateTargetVersionAttribute : ParameterValidationAttribute
    {
        private static readonly string[] PossibleValues = { "stable", "latest" };

#nullable disable
        public ValidateTargetVersionAttribute() : base(errorMessage: null)
        {
        }
#nullable enable

        public override ValidationResult Validate(ICommandParameterInfo info, object? value)
        {
            if (PossibleValues.Contains(value as string))
            {
                return ValidationResult.Success();
            }

            return ValidationResult.Error($"Value {value} for {info.PropertyName} is not valid. Valid values are: {string.Join(", ", PossibleValues)}");
        }
    }
}
