using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;

namespace Maba.Application.Services;

public class PricingService : IPricingService
{
    public decimal CalculateSuggestedPrice(
        decimal grams,
        decimal costPerGram,
        decimal printTimeHours,
        decimal hourlyRate,
        decimal qualityMultiplier)
    {
        var materialCost = grams * costPerGram;
        var machineCost = printTimeHours * hourlyRate * qualityMultiplier;
        return materialCost + machineCost;
    }

    /// <inheritdoc />
    public AdvancedPricingResult CalculateAdvancedPrice(
        decimal grams,
        decimal costPerGram,
        decimal printTimeHours,
        decimal hourlyRate,
        decimal qualityMultiplier,
        decimal profitMargin,
        decimal minimumPrice,
        decimal? roundToNearest)
    {
        var materialCost = grams * costPerGram;
        var machineCost = printTimeHours * hourlyRate;
        var baseCost = materialCost + machineCost;
        var adjustedCost = baseCost * qualityMultiplier;
        var afterMargin = adjustedCost * profitMargin;
        var minimumApplied = afterMargin < minimumPrice;
        var afterMinimum = minimumApplied ? minimumPrice : afterMargin;

        decimal finalSuggested = afterMinimum;
        decimal? roundStepUsed = null;
        var roundingApplied = false;
        if (roundToNearest.HasValue && roundToNearest.Value > 0)
        {
            roundStepUsed = roundToNearest.Value;
            var rounded = RoundToStep(afterMinimum, roundToNearest.Value);
            roundingApplied = rounded != afterMinimum;
            finalSuggested = rounded;
        }

        return new AdvancedPricingResult
        {
            MaterialCost = materialCost,
            MachineCost = machineCost,
            BaseCost = baseCost,
            AdjustedCost = adjustedCost,
            AfterMargin = afterMargin,
            AfterMinimum = afterMinimum,
            MinimumApplied = minimumApplied,
            FinalSuggested = finalSuggested,
            RoundStep = roundStepUsed,
            RoundingApplied = roundingApplied
        };
    }

    private static decimal RoundToStep(decimal value, decimal step)
    {
        if (step <= 0)
        {
            return value;
        }

        return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
    }
}
