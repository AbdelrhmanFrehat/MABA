namespace Maba.Application.Common.Models;

/// <summary>Breakdown from advanced 3D print pricing (suggestion only).</summary>
public sealed class AdvancedPricingResult
{
    public decimal MaterialCost { get; init; }
    public decimal MachineCost { get; init; }
    public decimal BaseCost { get; init; }
    /// <summary>Base × quality multiplier.</summary>
    public decimal AdjustedCost { get; init; }
    /// <summary>After quality × profit margin.</summary>
    public decimal AfterMargin { get; init; }
    /// <summary>After applying minimum price floor (before rounding).</summary>
    public decimal AfterMinimum { get; init; }
    public bool MinimumApplied { get; init; }
    /// <summary>Final suggested price (after optional rounding).</summary>
    public decimal FinalSuggested { get; init; }
    public decimal? RoundStep { get; init; }
    public bool RoundingApplied { get; init; }
}
