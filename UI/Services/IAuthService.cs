using UI.Models;
namespace UI.Services
{
    public interface IAuthService
    {
        Task<AccountStatus> RegisterAsync(Register register);
        Task<AccountStatus> LoginAsync(Login loging);
        Task LogoutAsync();
    }
}
