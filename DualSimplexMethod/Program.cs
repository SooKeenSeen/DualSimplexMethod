using DualSimplexMethod.ProjectUtilities;
using DualSimplexMethod.SimplexLibrary;
using Python.Runtime;
namespace DualSimplexMethod
{
    class Program
    {
        static void Main()
        {
            const string PATH = "DishesSet.csv";

            CSVUtility util = new(PATH);
            util.ConsoleInputConstraints();
            DualSimplex ds = new(util.BuildRightProvider());

            Console.WriteLine("Решение на C#");
            ds.TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) res);
            util.ResultInterpritate(res,new ConsoleOutputProvider());

            Console.WriteLine("\nРешение на Python");
            SimplexCSharpToPython cspt = new(util.BuildRightProvider());
            util.ResultInterpritate(cspt.PythonSolve(),new ConsoleOutputProvider());
        }
    }
}
