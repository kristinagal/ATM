using ATM.FileManager;

namespace ATM.Services
{
    public class CustomerService : ICustomerService //sitoj klasej - viskas, kas susije su vartotojo ivestimis
    {
        private readonly AtmService _atmService;

        public CustomerService(string filePath)
        {
            var atmDataService = new DataService(filePath); //kuriam nauja atmDataService objekta is gauto filePath
            _atmService = new AtmService(atmDataService); //atmDataService paduodame naujo AtmService objekto sukurimui, jis bus naudojamas sioje klaseje
                                                          //sukurus atmService iskart pasileidzia atmDataService.LoadATMData() ir pasikrauname duomenis is failo
        }

        public void Start()
        {
            while (true)
            {
                Console.Clear();
                if (!_atmService.IsLoggedIn) //jei niekas neprisijunges = nesuvedes korteles nr ir pin
                {
                    Console.WriteLine("Welcome to ATM!");
                    Console.WriteLine();
                    Console.WriteLine("Please enter card number:");
                    var cardNumber = Console.ReadLine();
                    Console.WriteLine("Please enter PIN:");
                    var pin = Console.ReadLine();

                    if (_atmService.Login(cardNumber, pin)) //tikrinam, ar vartotojo ivesti kortele ir pin yra musu duomenu faile
                                                            //jei taip - galesime pasiekti tos korteles duomenis per atmService
                                                            //t.y. susikuria Card objektas atmService'e
                    {
                        Console.WriteLine("Login successful.");
                        ShowMenu();
                    }
                }
                else
                {
                    ShowMenu();
                }
            }
        }

        private void ShowMenu() //pasiekamas tik is Start, kai AtmService yra sukurtas Card objektas
        {
            Console.Clear();
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Check Balance");
            Console.WriteLine("2. View Transaction History");
            Console.WriteLine("3. Cash In");
            Console.WriteLine("4. Cash Out");
            Console.WriteLine("5. Logout");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine($"Your balance is {_atmService.CheckBalance()} Eur");
                    //nieko papildomai ivesti nereikia, kreipiasi tiesiai i AtmService metoda
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "2":
                    var history = _atmService.GetTransactionHistory();
                    foreach (var transaction in history)
                    {
                        Console.WriteLine($"{transaction.Date}: {transaction.TransactionType} {transaction.Amount} Eur");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "3":
                    CashIn(); //cia reikalingos vartotojo ivestys, todel pirma kvieciame sios klases metoda, kuris iskvies atmService metoda
                    break;
                case "4":
                    CashOut();
                    break;
                case "5":
                    _atmService.Logout(); //istrinamas atmService klaseje sukurtas Card objektas
                    Console.WriteLine("You have been logged out.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }

        private void CashIn()
        {
            Console.Clear();
            var bills = new Dictionary<int, int>();
            var atmState = _atmService.GetATMBillInfo();
            var billDenominations = atmState.Keys.OrderBy(k => k).ToArray();

            foreach (var denomination in billDenominations)
            {
                bool validInput = false;
                while (!validInput)
                {
                    Console.WriteLine($"Enter the number of {denomination} Eur bills:");
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input)) // jei nieko neiveda - 0
                    {
                        bills[denomination] = 0;
                        validInput = true;
                    }
                    else if (int.TryParse(input, out int numberOfBills) && numberOfBills >= 0)
                    {
                        bills[denomination] = numberOfBills;
                        validInput = true;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid number of {denomination} Eur bills.");
                        Console.WriteLine("Press any key to retry...");
                        Console.ReadLine();
                    }
                }
            }

            _atmService.CashIn(bills);
            Console.WriteLine("Cash in successful.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void CashOut()
        {
            Console.Clear();
            Console.WriteLine("Available bills:");
            var atmState = _atmService.GetATMBillInfo();
            Console.WriteLine(string.Join(", ", atmState.Keys.OrderBy(k => k)));

            Console.WriteLine("Enter the amount to withdraw:");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please try again.");
                return;
            }

            try //cia dar neaisku, ar bankomate yra pakankamai kupiuru
            {
                var billsDispensed = _atmService.CashOut(amount);
                Console.WriteLine("Cash out successful. Bills dispensed:");
                foreach (var bill in billsDispensed)
                {
                    Console.WriteLine($"{bill.Key} Eur x {bill.Value}");
                }
            }
            catch (InvalidOperationException ex) //cia ismeta galimus errorus, kurie atsiranda is _atmService.CashOut(amount)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

}
