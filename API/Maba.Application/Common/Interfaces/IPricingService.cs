using Maba.Application.Common.Models;

namespace Maba.Application.Common.Interfaces;

/// <summary>3D print pricing helper (suggestion only; does not persist).</summary>
public interface IPricingService
{
    /// <summary>Legacy simple sum (legacy callers only).</summary>
    decimal CalculateSuggestedPrice(
        decimal grams,
        decimal costPerGram,
        decimal printTimeHours,
        decimal hourlyRate,
        decimal qualityMultiplier);

    /// <summary>
    /// Material + machine → × quality → × margin → floor to minimum → optional round.
    /// MachineCost = time × hourly (quality applied to base, not machine line alone).
    /// </summary>
    AdvancedPricingResult CalculateAdvancedPrice(
        decimal grams,
        decimal costPerGram,
        decimal printTimeHours,
        decimal hourlyRate,
        decimal qualityMultiplier,
        decimal profitMargin,
        decimal minimumPrice,
        decimal? roundToNearest);
}
