using System.Net.NetworkInformation;
using ATM.FileManager;
using ATM.Models;

namespace ATM.Services
{
    public class AtmService : IAtmService
    //cia bankomato veikimo logika, kai jau turime vartotojo input
    {
        private List<Card> _cards;
        private Dictionary<int, int> _bills;
        private IDataService _atmDataService;

        private Card currentCard;
        private int loginAttempts = 0;

        private const int MaxLoginAttempts = 3;
        private const decimal MaxWithdrawAmount = 1000;
        private const int MaxTransactionsPerDay = 10;
        private const int TransactionHistoryCount = 5;

        public bool IsLoggedIn //bool property, turi tik get, grąžina ar yra prisiloginus kortele
        {
            get
            {
                return currentCard != null;
            }
        }

        public AtmService(IDataService dataService) //konstruktorius, kuriam paduodam data servisa, ir tada su tuo servisu nukraunami duomenys is failo
        {
            _atmDataService = dataService;
            var atmData = _atmDataService.LoadATMData();
            _cards = atmData.Cards;
            _bills = atmData.BillDenominationCounts;
        }

        public bool Login(string cardNumber, string pin)
        {
            if (loginAttempts >= MaxLoginAttempts)
            {
                Console.WriteLine("Maximum login attempts exceeded. Exiting...");
                Environment.Exit(0);
            }

            currentCard = _cards.FirstOrDefault(c => c.CardNumber == cardNumber && c.Pin == pin);
            if (currentCard == null)
            {
                loginAttempts++;
                Console.WriteLine("Invalid card number or PIN.");
                return false; //sumuojami nesekmingi bandymai prisijungti ir kiekviena karta ivedus bloga korteles nr ar pin, metodas grazina false
            }

            loginAttempts = 0;
            return true;
        }

        public void Logout()
        {
            currentCard = null;
        }

        public decimal CheckBalance()
        {
            return currentCard.Balance;
        }

        public List<Transaction> GetTransactionHistory()
        {
            return currentCard.Transactions.OrderByDescending(t => t.Date).Take(TransactionHistoryCount).ToList();
        }

        public void CashIn(Dictionary<int, int> bills)
        {
            decimal totalAmount = 0;

            foreach (var bill in bills)
            {
                if (_bills.ContainsKey(bill.Key))
                {
                    _bills[bill.Key] += bill.Value;
                }
                else
                {
                    _bills[bill.Key] = bill.Value;
                }
                totalAmount += bill.Key * bill.Value;
            }

            currentCard.Balance += totalAmount;

            if (totalAmount != 0) //viskas suloginama
            {
                currentCard.Transactions.Add(new Transaction { Date = DateTime.Now, TransactionType = "Cash In", Amount = totalAmount });
                _atmDataService.SaveATMData(new AtmData { Cards = _cards, BillDenominationCounts = _bills });
            }
        }

        public Dictionary<int, int> CashOut(decimal amount)
        {
            if (amount > MaxWithdrawAmount)
            {
                throw new InvalidOperationException("Exceeds maximum withdrawal limit.");
            }

            if (amount > currentCard.Balance)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var todayTransactions = currentCard.Transactions.Where(t => t.Date.Date == DateTime.Now.Date).ToList(); //visos siandienos transakcijos
            if (todayTransactions.Count >= MaxTransactionsPerDay)
            {
                throw new InvalidOperationException("Exceeds maximum transactions per day.");
            }

            var billsToDispense = new Dictionary<int, int>(); //kiek ir kokiu kupiuru bus atiduodama
            var billsAvailable = _bills.OrderByDescending(b => b.Key).ToList(); //bankomate esancios kupiuros ir ju kiekiai
            decimal remainingAmount = amount;

            foreach (var bill in billsAvailable) //einama per visus nominalus mazejancia tvarka
            {
                int billValue = bill.Key;
                int availableCount = bill.Value;

                while (availableCount > 0 && remainingAmount >= billValue)
                {
                    remainingAmount -= billValue;
                    availableCount--;

                    if (!billsToDispense.ContainsKey(billValue))
                    {
                        billsToDispense[billValue] = 0;
                    }
                    billsToDispense[billValue]++;
                }

                _bills[billValue] = availableCount;
            }

            if (remainingAmount > 0)
            {
                throw new InvalidOperationException("Cannot dispense the requested amount with available bills.");
            }

            foreach (var bill in billsToDispense)
            {
                _bills[bill.Key] -= bill.Value; //jei viskas ok, atminusuojama is bankomate esanciu kupiuru
            }

            currentCard.Balance -= amount;
            if (amount != 0) //loginam jei ne 0
            {
                currentCard.Transactions.Add(new Transaction { Date = DateTime.Now, TransactionType = "Cash Out", Amount = amount });
                _atmDataService.SaveATMData(new AtmData { Cards = _cards, BillDenominationCounts = _bills });
            }

            return billsToDispense;
        }

        public Dictionary<int, int> GetATMBillInfo()
        {
            return _bills;
        }
    }
}
