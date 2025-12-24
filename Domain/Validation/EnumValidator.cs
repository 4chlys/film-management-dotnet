using System.ComponentModel.DataAnnotations;

namespace FilmManagement.BL.Domain.Validation
{
    public static class EnumValidator
    {
        public static ValidationResult DefinedValuesOnly(object value, ValidationContext context)
        {
            if (value is not Enum enumValue)
            {
                return new ValidationResult(
                    $"{context.MemberName} must be an Enum type. " +
                    $"EnumValidator cannot validate {value?.GetType().Name ?? "null"}.");
            }

            var enumType = enumValue.GetType();
            var isFlagsEnum = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
            
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var numericValue = Convert.ChangeType(enumValue, underlyingType);

            if (isFlagsEnum)
            {
                var allValidFlags = Enum.GetValuesAsUnderlyingType(enumType)
                    .Cast<object>()
                    .Aggregate(Convert.ChangeType(0, underlyingType), (acc, o) =>
                    {
                        dynamic accumulatedResult = acc;
                        dynamic enumAggregate = o;
                        return accumulatedResult | enumAggregate;
                    });
                
                dynamic val = numericValue;
                dynamic mask = allValidFlags;

                if ((val & ~mask) != 0)
                {
                    return new ValidationResult($"{context.MemberName} contains invalid flag values.");
                }
            }
            else
            {
                if (!Enum.IsDefined(enumType, value))
                {
                    return new ValidationResult($"{context.MemberName} is not a valid value.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
