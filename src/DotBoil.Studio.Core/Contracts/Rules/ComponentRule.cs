using DotBoil.Studio.Core.Enums;

namespace DotBoil.Studio.Core.Contracts.Rules;

public class ComponentRule
{
    public string Name { get; set; } = "New Rule";

    public RuleGroupOperator GroupOperator { get; set; } = RuleGroupOperator.All;

    public List<RuleCondition> Conditions { get; set; } = new();

    public string ValidationMessage { get; set; } = string.Empty;
}
