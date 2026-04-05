using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Services;

public class DocumentNumberService : IDocumentNumberService
{
    private readonly ApplicationDbContext _context;

    public DocumentNumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(string documentType, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.DocumentSequences
            .FirstOrDefaultAsync(x => x.DocumentType == documentType && x.IsActive, cancellationToken);

        if (sequence == null)
        {
            throw new KeyNotFoundException($"No active document sequence found for '{documentType}'.");
        }

        var currentYear = DateTime.UtcNow.Year;
        if (sequence.IncludeYear && sequence.CurrentYear != currentYear)
        {
            sequence.CurrentYear = currentYear;
            sequence.LastNumber = 0;
        }

        sequence.LastNumber += 1;
        await _context.SaveChangesAsync(cancellationToken);

        var numberPart = sequence.LastNumber.ToString().PadLeft(sequence.PadLength, '0');
        return sequence.IncludeYear
            ? $"{sequence.Prefix}{sequence.Separator}{sequence.CurrentYear}{sequence.Separator}{numberPart}"
            : $"{sequence.Prefix}{sequence.Separator}{numberPart}";
    }
}
