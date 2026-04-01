using DotBoil.Studio.Core.Enums;

namespace DotBoil.Studio.Core.Contracts.Rules;

public class RuleCondition
{
    public RuleConditionType Type { get; set; } = RuleConditionType.ComponentValue;

    public string ComponentId { get; set; } = string.Empty;

    public RuleComparisonOperator Operator { get; set; } = RuleComparisonOperator.Equals;

    public string Value { get; set; } = string.Empty;
}
