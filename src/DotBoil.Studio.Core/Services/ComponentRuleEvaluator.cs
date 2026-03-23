using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Contracts.Rules;
using DotBoil.Studio.Core.Enums;

namespace DotBoil.Studio.Core.Services;

public static class ComponentRuleEvaluator
{
    public static void Apply(BaseComponent currentComponent, ComponentContext context)
    {
        var isVisible = EvaluateVisibility(currentComponent, context);
        var isEnabled = EvaluateEnabled(currentComponent, context);
        var validationMessage = EvaluateValidationMessage(currentComponent, context);

        currentComponent.IsVisible = isVisible;
        currentComponent.IsEnabled = isEnabled;
        currentComponent.ValidationMessage = validationMessage;
        currentComponent.HasValidationError = !string.IsNullOrWhiteSpace(validationMessage);
    }

    private static bool EvaluateVisibility(BaseComponent component, ComponentContext context)
    {
        var showMatched = component.ShowRules.Any(rule => IsRuleMatched(rule, context));
        var hideMatched = component.HideRules.Any(rule => IsRuleMatched(rule, context));

        var visible = component.ShowRules.Count == 0 || showMatched;
        if (hideMatched)
            visible = false;

        return visible;
    }

    private static bool EvaluateEnabled(BaseComponent component, ComponentContext context)
    {
        var enableMatched = component.EnableRules.Any(rule => IsRuleMatched(rule, context));
        var disableMatched = component.DisableRules.Any(rule => IsRuleMatched(rule, context));

        var enabled = component.EnableRules.Count == 0 || enableMatched;
        if (disableMatched)
            enabled = false;

        return enabled;
    }

    private static string EvaluateValidationMessage(BaseComponent component, ComponentContext context)
    {
        foreach (var rule in component.ValidationRules)
        {
            if (IsRuleMatched(rule, context))
                return rule.ValidationMessage ?? string.Empty;
        }

        return string.Empty;
    }

    private static bool IsRuleMatched(ComponentRule rule, ComponentContext context)
    {
        if (rule.Conditions == null || rule.Conditions.Count == 0)
            return false;

        var evaluations = rule.Conditions.Select(condition => EvaluateCondition(condition, context)).ToList();
        return rule.GroupOperator == RuleGroupOperator.All ? evaluations.All(x => x) : evaluations.Any(x => x);
    }

    private static bool EvaluateCondition(RuleCondition condition, ComponentContext context)
    {
        switch (condition.Type)
        {
            case RuleConditionType.ComponentValue:
                return EvaluateComponentValueCondition(condition, context);
            case RuleConditionType.ComponentHasValue:
                return EvaluateHasValueCondition(condition, context);
            case RuleConditionType.ComponentIsEmpty:
                return EvaluateIsEmptyCondition(condition, context);
            case RuleConditionType.UserRole:
                return EvaluateUserRoleCondition(condition, context);
            default:
                return false;
        }
    }

    private static bool EvaluateComponentValueCondition(RuleCondition condition, ComponentContext context)
    {
        var componentValue = GetComponentOutputValue(condition.ComponentId, context);
        if (componentValue == null)
            return false;

        var left = componentValue.ToString() ?? string.Empty;
        var right = condition.Value ?? string.Empty;

        if (decimal.TryParse(left, out var leftDecimal) && decimal.TryParse(right, out var rightDecimal))
            return CompareNumbers(leftDecimal, rightDecimal, condition.Operator);

        return CompareStrings(left, right, condition.Operator);
    }

    private static bool EvaluateHasValueCondition(RuleCondition condition, ComponentContext context)
    {
        var componentValue = GetComponentOutputValue(condition.ComponentId, context);
        return HasValue(componentValue);
    }

    private static bool EvaluateIsEmptyCondition(RuleCondition condition, ComponentContext context)
    {
        var componentValue = GetComponentOutputValue(condition.ComponentId, context);
        return !HasValue(componentValue);
    }

    private static bool EvaluateUserRoleCondition(RuleCondition condition, ComponentContext context)
    {
        if (string.IsNullOrWhiteSpace(condition.Value))
            return false;

        return context.UserRoles.Any(role => string.Equals(role, condition.Value, StringComparison.OrdinalIgnoreCase));
    }

    private static object? GetComponentOutputValue(string componentId, ComponentContext context)
    {
        if (string.IsNullOrWhiteSpace(componentId) || context.Components == null || context.Components.Count == 0)
            return null;

        var component = context.Components.FirstOrDefault(c =>
            string.Equals(c.Id, componentId, StringComparison.OrdinalIgnoreCase));

        if (component == null)
            return null;

        var outputProperty = component.GetType()
            .GetProperties()
            .FirstOrDefault(property => property.GetCustomAttributes(typeof(FieldOutputPropertyAttribute), true).Any());

        return outputProperty?.GetValue(component);
    }

    private static bool CompareStrings(string left, string right, RuleComparisonOperator @operator)
    {
        var compare = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        return @operator switch
        {
            RuleComparisonOperator.Equals => compare == 0,
            RuleComparisonOperator.NotEquals => compare != 0,
            RuleComparisonOperator.GreaterThan => compare > 0,
            RuleComparisonOperator.GreaterThanOrEqual => compare >= 0,
            RuleComparisonOperator.LessThan => compare < 0,
            RuleComparisonOperator.LessThanOrEqual => compare <= 0,
            _ => false
        };
    }

    private static bool CompareNumbers(decimal left, decimal right, RuleComparisonOperator @operator)
    {
        return @operator switch
        {
            RuleComparisonOperator.Equals => left == right,
            RuleComparisonOperator.NotEquals => left != right,
            RuleComparisonOperator.GreaterThan => left > right,
            RuleComparisonOperator.GreaterThanOrEqual => left >= right,
            RuleComparisonOperator.LessThan => left < right,
            RuleComparisonOperator.LessThanOrEqual => left <= right,
            _ => false
        };
    }

    private static bool HasValue(object? value)
    {
        if (value == null)
            return false;

        if (value is string text)
            return !string.IsNullOrWhiteSpace(text);

        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var enumerator = enumerable.GetEnumerator();
            return enumerator.MoveNext();
        }

        return true;
    }
}
