using BankingAppBackend.Dat;
using BankingAppBackend.Models;
using BankingAppBackend.Utils;
using Microsoft.Extensions.Options;
using BankingAppBackend.Services.Interfaces;
using Newtonsoft.Json;
using static BankingAppBackend.Models.Transaction;

namespace BankingAppBackend.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private MyDbContext _dbContext;

        private ILogger<TransactionService> _logger;
        private IUserService _userService;
        private AppSettings _settings;
        private static string _bankSettlementAccount;

        public TransactionService(MyDbContext dbContext, ILogger<TransactionService> logger, IUserService userService, IOptions<AppSettings> settings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _settings = settings.Value;

            _bankSettlementAccount = _settings.NetCoreBankSettlementAccount;

        }

        public Response CreateNewTransaction(Transaction transaction)
        {
            Response response = new Response();
            try
            {
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();
                response.ResponseCode = "00";

                response.ResponseMessage = "Transaction created successfully!";
                response.Data = null;
            }
            catch (Exception ex)
            {

                _logger.LogError($"AN ERROR OCCURRED => {ex.Message}");
            }
            return response;

        }

        public Response FindTransactionByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Response MakeDeposit(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; //our Bank Settlement Account
            Account destinationAccount; //individual
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(_bankSettlementAccount);
                destinationAccount = _userService.GetByAccountNumber(AccountNumber);

                sourceAccount.CurrentAccountBalance -= Amount;
                destinationAccount.CurrentAccountBalance += Amount;

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    //sso there was an update
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;

                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Deposit;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();


            return response;

        }

        public Response MakeFundsTransfer(string FromEmail, string ToEmail, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; // our current authenticated customer's account
            Account destinationAccount; // target account where money is being sent to
            Transaction transaction = new Transaction();

            // Authenticate the user using the provided transaction pin
            var authenticateUser = _userService.Authenticate(TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Pin");
            }

            // Find the source and destination accounts based on email addresses
            sourceAccount = _userService.GetByEmail(FromEmail);
            destinationAccount = _userService.GetByEmail(ToEmail);

            // Validate accounts
            if (sourceAccount == null)
            {
                response.ResponseCode = "01";
                response.ResponseMessage = "Source account not found.";
                return response;
            }

            if (destinationAccount == null)
            {
                response.ResponseCode = "02";
                response.ResponseMessage = "Destination account not found.";
                return response;
            }

            if (FromEmail.Equals(ToEmail))
            {
                response.ResponseCode = "03";
                response.ResponseMessage = "You cannot transfer money to yourself.";
                return response;
            }

            if (Amount <= 0)
            {
                response.ResponseCode = "04";
                response.ResponseMessage = "Transfer amount must be greater than zero.";
                return response;
            }

            if (sourceAccount.CurrentAccountBalance < Amount)
            {
                response.ResponseCode = "05";
                response.ResponseMessage = "Insufficient funds.";
                return response;
            }

            // Process the funds transfer
            try
            {
                sourceAccount.CurrentAccountBalance -= Amount; // remove the amount from the source account
                destinationAccount.CurrentAccountBalance += Amount; // add the amount to the destination account

                // Save changes
                _dbContext.Accounts.Update(sourceAccount);
                _dbContext.Accounts.Update(destinationAccount);

                // Create and save the transaction record
                transaction.TransactionStatus = TranStatus.Success;
                transaction.TransactionDate = DateTime.Now;
                transaction.TransactionType = TranType.Transfer;
                transaction.TransactionAmount = Amount;
                transaction.TransactionSourceAccount = FromEmail;
                transaction.TransactionDestinationAccount = ToEmail;
                transaction.TransactionParticulars = $"Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE => {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();

                response.ResponseCode = "00";
                response.ResponseMessage = "Transaction Successful!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
                response.ResponseCode = "06";
                response.ResponseMessage = "Transaction failed due to an internal error.";
            }

            return response;
        }


        public Response MakeWithdrawal(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; //individual
            Account destinationAccount; //our Bank Settlement Account
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(AccountNumber);
                destinationAccount = _userService.GetByAccountNumber(_bankSettlementAccount);

                sourceAccount.CurrentAccountBalance -= Amount; //remove the tranamount from the customer's balance
                destinationAccount.CurrentAccountBalance += Amount; //add tranamount to our bankSettlement...

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    //so there was an update in the context State
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;

                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Withdrawal;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();


            return response;
        }
        // Fetch transaction history by email
        public List<Transaction> GetTransactionHistoryByEmail(string email)
        {
            // Get the account using the provided email
            var account = _dbContext.Accounts.SingleOrDefault(a => a.Email == email);
            if (account == null) return null;

            // Fetch the transaction history related to this account
            var transactions = _dbContext.Transactions
                .Where(t => t.TransactionSourceAccount == account.Email || t.TransactionDestinationAccount == account.Email)
                .OrderByDescending(t => t.TransactionDate) // Order by most recent first
                .ToList();

            return transactions;
        }
    }
}
