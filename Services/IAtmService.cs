using ATM.Models;

namespace ATM.Services
{
    public interface IAtmService
    {
        bool IsLoggedIn { get; }

        Dictionary<int, int> CashOut(decimal amount);
        decimal CheckBalance();
        List<Transaction> GetTransactionHistory();
        void CashIn(Dictionary<int, int> bills);
        bool Login(string cardNumber, string pin);
        void Logout();
        Dictionary<int, int> GetATMBillInfo();
    }
}