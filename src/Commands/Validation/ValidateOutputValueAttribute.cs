using PackCheck.Data;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValidateOutputValueAttribute(string errorMessage)
    : ParameterValidationAttribute(errorMessage)
{
    public override ValidationResult Validate(CommandParameterContext context)
    {
        return OutputFormat.All.Contains(context.Value as string)
            ? ValidationResult.Success()
            : ValidationResult.Error(
                string.Format(
                    "Value '{0}' for {1} is not valid. Valid values are: {2}",
                    context.Value,
                    context.Parameter.PropertyName,
                    string.Join(", ", OutputFormat.All)
                )
            );
    }
}
