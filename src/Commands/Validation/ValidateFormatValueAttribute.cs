using System;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValidateFormatValueAttribute(string errorMessage)
    : ParameterValidationAttribute(errorMessage)
{
    private static readonly string[] PossibleValues =  ["group"];

    public override ValidationResult Validate(CommandParameterContext context)
    {
        return PossibleValues.Contains(context.Value as string)
            ? ValidationResult.Success()
            : ValidationResult.Error(
                string.Format(
                    "Value '{0}' for {1} is not valid. Valid values are: {2}",
                    context.Value,
                    context.Parameter.PropertyName,
                    string.Join(", ", PossibleValues)
                )
            );
    }
}
