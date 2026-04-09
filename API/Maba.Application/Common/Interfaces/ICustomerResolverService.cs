namespace Maba.Application.Common.Interfaces;

/// <summary>
/// Finds an existing Customer linked to a website user or email, or creates one on first contact.
/// </summary>
public interface ICustomerResolverService
{
    /// <summary>
    /// Resolves (or creates) a Customer record from a service request submission.
    /// Priority order:
    ///   1. If <paramref name="userId"/> is set → look for Customer.UserId == userId
    ///   2. Else if <paramref name="email"/> is set → look for Customer.Email == email
    ///   3. If found by email and userId is set → link the userId, update and return
    ///   4. If nothing found → create a new Customer, link userId if provided
    /// </summary>
    Task<Guid> ResolveAsync(
        Guid? userId,
        string name,
        string? email,
        string? phone,
        CancellationToken cancellationToken = default);
}
