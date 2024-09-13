using BankingAppBackend.Models;
using System.Collections.Generic;

namespace BankingAppBackend.Services.Interfaces
{
    public interface IUserService
    {
        string Authenticate(string Pin);
        IEnumerable<Account> GetAllAccounts();
        Account Create(Account account, string Pin, string ConfirmPin);
        void Update(Account account, string Pin = null);
        void Delete(int Id);
        Account GetById(int Id);
        Account GetByAccountNumber(string AccountNumber);
        Account GetByEmail(string email);
    }
}
