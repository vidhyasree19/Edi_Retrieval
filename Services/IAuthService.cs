using EdiRetrieval.Models;
using System.Threading.Tasks;

namespace EdiRetrieval.Services
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string email, string password);
    }
}
