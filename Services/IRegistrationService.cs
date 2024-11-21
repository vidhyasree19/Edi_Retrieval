using EdiRetrieval.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EdiRetrieval.Services
{
    public interface IRegistrationService
    {
        Task<string> RegisterUserAsync(Register register);
    }
}
