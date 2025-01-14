using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplexMethod.SimplexLibrary
{
    public class ConsoleOutputProvider : IDataOutputProvider
    {
        public void Output(string data)
        {
            Console.Write(data);
        }
    }
}
