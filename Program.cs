using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATM.Services;

namespace ATM
{
    public class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\Users\\Kristina\\source\\repos\\ATM\\FileManager\\data.json";
            CustomerService atm = new CustomerService(filePath);
            atm.Start();
        }
    }
}
