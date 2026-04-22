using System.Net.Http;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IAuthSessionService
{
    bool IsAuthenticated { get; }
    AuthUser? CurrentUser { get; }
    AuthSession? CurrentSession { get; }
    event EventHandler? AuthenticationChanged;

    Task<bool> LoginAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default);
    void Logout();
    HttpClient CreateHttpClient();
}
