using ATM.Models;

namespace ATM.FileManager
{
    public class AtmData
    {
        public List<Card> Cards { get; set; } = new List<Card>();
        public Dictionary<int, int> BillDenominationCounts { get; set; } = new Dictionary<int, int>();
    }
}
