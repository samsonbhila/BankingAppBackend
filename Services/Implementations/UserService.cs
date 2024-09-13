using BankingAppBackend.Dat;
using BankingAppBackend.Models;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using BankingAppBackend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace BankingAppBackend.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MyDbContext _dbContext;
        private ILogger<UserService> _logger;

        public UserService(MyDbContext dbContext,ILogger<UserService> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration=configuration;
        }

        public string Authenticate(string pin)
        {
            // Load all accounts from the database into memory
            var accounts = _dbContext.Accounts.ToList();

            // Find the account that matches the given pin
            var account = accounts.SingleOrDefault(a => VerifyPinHash(pin, a.PinStoredHash, a.PinStoredSalt));

            if (account == null)
                return null;

            // Create JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(_configuration["JwtSettings:SecretKey"]); // Decode the Base64 key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Email),                    
            new Claim("FirstName", account.FirstName)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static bool VerifyPinHash(string pin, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(pin)) throw new ArgumentNullException(nameof(pin));

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
                return computedHash.SequenceEqual(storedHash);
            }
        }


        // Create account method
        public Account Create(Account account, string pin, string confirmPin)
        {
            if (string.IsNullOrWhiteSpace(pin)) throw new ArgumentNullException("Pin cannot be empty");
            if (_dbContext.Accounts.Any(x => x.Email == account.Email)) throw new ApplicationException("A user with this email already exists.");
            if (!pin.Equals(confirmPin)) throw new ApplicationException("Pins do not match.");

            byte[] pinHash, pinSalt;
            CreatePinHash(pin, out pinHash, out pinSalt);

            account.PinStoredHash = pinHash;
            account.PinStoredSalt = pinSalt;

            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges();

            return account;
        }


        // Hash creation for pin
        private static void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt)
        {
            if (string.IsNullOrEmpty(pin)) throw new ArgumentNullException(nameof(pin));

            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
            }
        }

        // Deletes an account
        public void Delete(int id)
        {
            var account = _dbContext.Accounts.Find(id);
            if (account != null)
            {
                _dbContext.Accounts.Remove(account);
                _dbContext.SaveChanges();
            }
        }

        // Retrieves all accounts
        public IEnumerable<Account> GetAllAccounts()
        {
            return _dbContext.Accounts.ToList();
        }

        // Retrieves account by ID
        public Account GetById(int id)
        {
            return _dbContext.Accounts.FirstOrDefault(x => x.Id == id);
        }

        // Updates account details and optionally updates the pin
        public void Update(Account account, string pin = null)
        {
            var accountToBeUpdated = _dbContext.Accounts.Find(account.Id);
            if (accountToBeUpdated == null) throw new ApplicationException("Account not found");

            // Update Email if changed and unique
            if (!string.IsNullOrWhiteSpace(account.Email) && account.Email != accountToBeUpdated.Email)
            {
                if (_dbContext.Accounts.Any(x => x.Email == account.Email)) throw new ApplicationException($"Email {account.Email} is already taken");
                accountToBeUpdated.Email = account.Email;
            }

            // Update PhoneNumber if changed and unique
            if (!string.IsNullOrWhiteSpace(account.PhoneNumber) && account.PhoneNumber != accountToBeUpdated.PhoneNumber)
            {
                if (_dbContext.Accounts.Any(x => x.PhoneNumber == account.PhoneNumber)) throw new ApplicationException($"PhoneNumber {account.PhoneNumber} is already taken");
                accountToBeUpdated.PhoneNumber = account.PhoneNumber;
            }

            // Update pin if provided
            if (!string.IsNullOrWhiteSpace(pin))
            {
                byte[] pinHash, pinSalt;
                CreatePinHash(pin, out pinHash, out pinSalt);
                accountToBeUpdated.PinStoredHash = pinHash;
                accountToBeUpdated.PinStoredSalt = pinSalt;
            }

            _dbContext.Accounts.Update(accountToBeUpdated);
            _dbContext.SaveChanges();
        }

        // Retrieve an account by AccountNumber
        public Account GetByAccountNumber(string accountNumber)
        {
            return _dbContext.Accounts.SingleOrDefault(x => x.AccountNumberGenerated == accountNumber);
        }

        // Retrieve an account by email
        public Account GetByEmail(string email)
        {
            return _dbContext.Accounts.SingleOrDefault(a => a.Email == email);
        }

    }
}
