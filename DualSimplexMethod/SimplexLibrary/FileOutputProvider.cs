using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplexMethod.SimplexLibrary
{
    public class FileOutputProvider(string path) : IDataOutputProvider
    {
        private readonly string _path = path;

        public void Output(string data)
        {
            
            File.AppendAllText(_path, data);
        }
    }
}
