using System.Globalization;
using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class UpdateEraConfigCommandValidator : FluentMessageValidator<UpdateEraConfigCommand>
{
    public UpdateEraConfigCommandValidator()
    {
        RuleFor(command => command.ExpectedVersion)
            .GreaterThan(0)
            .LessThan(long.MaxValue);
        RuleFor(command => command.Config)
            .NotNull()
            .SetValidator(new EraConfigDtoValidator());
    }
}

public class EraConfigDtoValidator : AbstractValidator<EraConfigDto>
{
    private static readonly string[] RequiredConfigKeys =
    {
        EraConfigurationKeys.AttributeMinimum,
        EraConfigurationKeys.AttributeMaximum,
        EraConfigurationKeys.BaseStrength,
        EraConfigurationKeys.BaseConstitution,
        EraConfigurationKeys.BaseAgility,
        EraConfigurationKeys.BaseHealth,
        EraConfigurationKeys.StrengthBonusPercent,
        EraConfigurationKeys.ConstitutionBonusPercent,
        EraConfigurationKeys.AgilityBonusPercent,
        EraConfigurationKeys.DefaultWarehouseCapacity,
        EraConfigurationKeys.CharacterLimit
    };

    public EraConfigDtoValidator()
    {
        RuleFor(configuration => configuration.Items)
            .NotEmpty()
            .Must(HaveUniqueKeys);
        RuleForEach(configuration => configuration.Items)
            .SetValidator(new EraConfigurationItemDtoValidator());
        RuleFor(configuration => configuration)
            .Must(HaveRequiredConfigKeys)
            .Must(HaveValidCoreValues);
    }

    private static bool HaveUniqueKeys(List<EraConfigurationItemDto>? items)
    {
        if (items == null)
        {
            return false;
        }

        return items
            .Select(item => item.Key)
            .Distinct(StringComparer.Ordinal)
            .Count() == items.Count;
    }

    private static bool HaveRequiredConfigKeys(EraConfigDto configuration)
    {
        if (configuration.Items == null)
        {
            return false;
        }

        var keys = configuration.Items
            .Select(item => item.Key)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var requiredConfigKey in RequiredConfigKeys)
        {
            if (!keys.Contains(requiredConfigKey))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveValidCoreValues(EraConfigDto configuration)
    {
        if (configuration.Items == null)
        {
            return false;
        }

        if (!TryGetDecimal(configuration, EraConfigurationKeys.AttributeMinimum, out var attributeMinimum) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.AttributeMaximum, out var attributeMaximum) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.BaseStrength, out var baseStrength) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.BaseConstitution, out var baseConstitution) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.BaseAgility, out var baseAgility) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.BaseHealth, out var baseHealth) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.StrengthBonusPercent, out var strengthBonusPercent) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.ConstitutionBonusPercent, out var constitutionBonusPercent) ||
            !TryGetDecimal(configuration, EraConfigurationKeys.AgilityBonusPercent, out var agilityBonusPercent) ||
            !TryGetInteger(configuration, EraConfigurationKeys.DefaultWarehouseCapacity, out var warehouseCapacity))
        {
            return false;
        }

        var attributesAreValid = attributeMinimum >= 0 &&
                                 attributeMaximum > attributeMinimum &&
                                 baseStrength >= attributeMinimum &&
                                 baseStrength <= attributeMaximum &&
                                 baseConstitution >= attributeMinimum &&
                                 baseConstitution <= attributeMaximum &&
                                 baseAgility >= attributeMinimum &&
                                 baseAgility <= attributeMaximum;
        var derivedValuesAreValid = baseHealth > 0 &&
                                    strengthBonusPercent >= 0 &&
                                    constitutionBonusPercent >= 0 &&
                                    agilityBonusPercent >= 0;
        if (!TryGetInteger(configuration, EraConfigurationKeys.CharacterLimit, out var characterLimit) || characterLimit < 1)
        {
            return false;
        }
        return attributesAreValid && derivedValuesAreValid && warehouseCapacity >= 0;
    }

    private static bool TryGetDecimal(
        EraConfigDto configuration,
        string key,
        out decimal value)
    {
        var item = configuration.Items.SingleOrDefault(
            configurationItem => string.Equals(configurationItem.Key, key, StringComparison.Ordinal));
        if (item == null || !string.Equals(item.ValueType, "decimal", StringComparison.Ordinal))
        {
            value = default;
            return false;
        }

        return decimal.TryParse(
            item.Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool TryGetInteger(
        EraConfigDto configuration,
        string key,
        out int value)
    {
        var item = configuration.Items.SingleOrDefault(
            configurationItem => string.Equals(configurationItem.Key, key, StringComparison.Ordinal));
        if (item == null || !string.Equals(item.ValueType, "integer", StringComparison.Ordinal))
        {
            value = default;
            return false;
        }

        return int.TryParse(
            item.Value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);
    }
}

public class EraConfigurationItemDtoValidator : AbstractValidator<EraConfigurationItemDto>
{
    public EraConfigurationItemDtoValidator()
    {
        RuleFor(item => item.Key)
            .NotEmpty()
            .MaximumLength(128);
        RuleFor(item => item.ValueType)
            .NotEmpty()
            .MaximumLength(32)
            .Must(IsSupportedValueType);
        RuleFor(item => item.Value)
            .NotNull()
            .Must(HaveValidValue);
        RuleFor(item => item.Description)
            .MaximumLength(256);
    }

    private static bool IsSupportedValueType(string valueType)
    {
        return valueType == "decimal" ||
               valueType == "integer" ||
               valueType == "boolean" ||
               valueType == "string";
    }

    private static bool HaveValidValue(EraConfigurationItemDto item, string value)
    {
        if (item.ValueType == "decimal")
        {
            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var decimalValue);
        }

        if (item.ValueType == "integer")
        {
            return long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var integerValue);
        }

        if (item.ValueType == "boolean")
        {
            return bool.TryParse(value, out var booleanValue);
        }

        return item.ValueType == "string";
    }
}
