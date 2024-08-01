namespace ATM.Models
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
