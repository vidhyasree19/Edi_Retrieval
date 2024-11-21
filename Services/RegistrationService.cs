using EdiRetrieval.Data;
using EdiRetrieval.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdiRetrieval.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> RegisterUserAsync(Register register)
        {
            var existingUser = await _context.Registers
                                              .FirstOrDefaultAsync(u => u.Email == register.Email);
            if (existingUser != null)
            {
                return "User already exists!";
            }

            if (string.IsNullOrEmpty(register.Password))
            {
                return "Password is required!";
            }

            await _context.Registers.AddAsync(register);
            await _context.SaveChangesAsync();

            return "User registered successfully!";
        }


    }
}
