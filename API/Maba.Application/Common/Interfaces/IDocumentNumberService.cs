namespace Maba.Application.Common.Interfaces;

public interface IDocumentNumberService
{
    Task<string> GenerateNextAsync(string documentType, CancellationToken cancellationToken = default);
}
