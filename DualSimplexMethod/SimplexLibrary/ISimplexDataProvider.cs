using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplexMethod.SimplexLibrary
{
    public interface ISimplexDataProvider
    {
        public List<Constraint> GetConstraints();
        public TargetFunctional GetTargetFunctional();
        public string[] GetDescriptions();
    }

    public interface IDataOutputProvider
    {
        public void Output(string data);
    }
}
