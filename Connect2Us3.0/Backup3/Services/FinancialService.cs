using book2us.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace book2us.Services
{
    public class FinancialService
    {
        private readonly Book2UsContext _db;
        private readonly string _encryptionKey;

        public FinancialService(Book2UsContext db)
        {
            _db = db;
            _encryptionKey = "YourSecretEncryptionKey1234567890123456"; // In production, store this securely
        }

        public string EncryptCardNumber(string cardNumber)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                    aes.GenerateIV();

                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encrypted = encryptor.TransformFinalBlock(
                        Encoding.UTF8.GetBytes(cardNumber), 0, cardNumber.Length);

                    return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encrypted);
                }
            }
            catch
            {
                return cardNumber; // Fallback for encryption errors
            }
        }

        public string DecryptCardNumber(string encryptedData)
        {
            try
            {
                string[] parts = encryptedData.Split(':');
                if (parts.Length != 2) return encryptedData;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                    aes.IV = Convert.FromBase64String(parts[0]);

                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    byte[] decrypted = decryptor.TransformFinalBlock(
                        Convert.FromBase64String(parts[1]), 0, Convert.FromBase64String(parts[1]).Length);

                    return Encoding.UTF8.GetString(decrypted);
                }
            }
            catch
            {
                return encryptedData; // Fallback for decryption errors
            }
        }

        public Transaction CreateTransaction(int userId, TransactionType type, PaymentMethod paymentMethod, 
            decimal amount, string description = null, int? bankAccountId = null, int? cardId = null)
        {
            var fee = CalculateTransactionFee(type, paymentMethod, amount);
            var netAmount = amount - fee;

            var transaction = new Transaction
            {
                UserId = userId,
                Type = type,
                Status = TransactionStatus.Pending,
                PaymentMethod = paymentMethod,
                Amount = amount,
                Fee = fee,
                NetAmount = netAmount,
                Description = description,
                BankAccountId = bankAccountId,
                CardId = cardId,
                TransactionReference = GenerateTransactionReference(),
                CreatedDate = DateTime.Now
            };

            _db.Transactions.Add(transaction);
            _db.SaveChanges();

            return transaction;
        }

        public bool ProcessWalletTransaction(Transaction transaction)
        {
            try
            {
                var wallet = _db.Wallets.SingleOrDefault(w => w.UserId == transaction.UserId);
                if (wallet == null || !wallet.IsActive)
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.Description = "Wallet not found or inactive";
                    _db.SaveChanges();
                    return false;
                }

                if (transaction.Type == TransactionType.Withdrawal && wallet.Balance < transaction.Amount)
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.Description = "Insufficient balance";
                    _db.SaveChanges();
                    return false;
                }

                // Process transaction based on type
                switch (transaction.Type)
                {
                    case TransactionType.Deposit:
                        wallet.Balance += transaction.NetAmount;
                        break;
                    case TransactionType.Withdrawal:
                        wallet.Balance -= transaction.Amount;
                        break;
                }

                wallet.LastTransactionDate = DateTime.Now;
                transaction.Status = TransactionStatus.Completed;
                transaction.ProcessedDate = DateTime.Now;
                
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.Description = ex.Message;
                _db.SaveChanges();
                return false;
            }
        }

        public WithdrawalRequest CreateWithdrawalRequest(int userId, int bankAccountId, decimal amount)
        {
            var fee = CalculateWithdrawalFee(amount);
            var netAmount = amount - fee;

            var withdrawalRequest = new WithdrawalRequest
            {
                UserId = userId,
                BankAccountId = bankAccountId,
                Amount = amount,
                Fee = fee,
                NetAmount = netAmount,
                Status = WithdrawalStatus.Pending,
                RequestedDate = DateTime.Now,
                RequestedBy = _db.ApplicationUsers.Find(userId)?.UserName
            };

            _db.WithdrawalRequests.Add(withdrawalRequest);
            _db.SaveChanges();

            return withdrawalRequest;
        }

        public bool ProcessWithdrawalRequest(WithdrawalRequest request)
        {
            try
            {
                // Create transaction for withdrawal
                var transaction = CreateTransaction(
                    request.UserId,
                    TransactionType.Withdrawal,
                    PaymentMethod.BankTransfer,
                    request.Amount,
                    $"Withdrawal to bank account ending in {GetBankAccountLastFourDigits(request.BankAccountId)}",
                    request.BankAccountId
                );

                // Process wallet transaction
                if (ProcessWalletTransaction(transaction))
                {
                    request.Status = WithdrawalStatus.Completed;
                    request.TransactionId = transaction.TransactionId;
                    request.ProcessedDate = DateTime.Now;
                    _db.SaveChanges();
                    return true;
                }
                else
                {
                    request.Status = WithdrawalStatus.Failed;
                    request.Notes = "Wallet transaction failed";
                    _db.SaveChanges();
                    return false;
                }
            }
            catch (Exception ex)
            {
                request.Status = WithdrawalStatus.Failed;
                request.Notes = ex.Message;
                _db.SaveChanges();
                return false;
            }
        }

        private decimal CalculateTransactionFee(TransactionType type, PaymentMethod method, decimal amount)
        {
            // Implement your fee calculation logic here
            if (type == TransactionType.Withdrawal)
            {
                return Math.Min(amount * 0.02m, 10.00m); // 2% fee, max $10
            }
            return 0;
        }

        private decimal CalculateWithdrawalFee(decimal amount)
        {
            return Math.Min(amount * 0.02m, 10.00m); // 2% fee, max $10
        }

        private string GenerateTransactionReference()
        {
            return $"TXN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        private string GetBankAccountLastFourDigits(int bankAccountId)
        {
            var bankAccount = _db.BankAccounts.Find(bankAccountId);
            return bankAccount?.AccountNumber?.Substring(Math.Max(0, bankAccount.AccountNumber.Length - 4)) ?? "****";
        }
    }
}