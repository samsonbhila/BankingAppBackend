﻿using System.ComponentModel.DataAnnotations;

namespace BankingAppBackend.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public string TransactionUniqueReference { get; set; }
        public decimal TransactionAmount { get; set; }
        public TranStatus TransactionStatus { get; set; }
        public bool IsSuccessful => TransactionStatus.Equals(TranStatus.Success);
        public string TransactionSourceAccount { get; set; }
        public string TransactionDestinationAccount { get; set; }
        public string TransactionParticulars { get; set; }
        public TranType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }


        public Transaction()
        {
            TransactionUniqueReference = $"{Guid.NewGuid().ToString().Replace("-", "").Substring(1, 17)}";
        }
        public enum TranStatus
        {
            Failed,
            Success,
            Error
        }

        public enum TranType
        {
            Deposit,
            Withdrawal,
            Transfer
        }

    }
}