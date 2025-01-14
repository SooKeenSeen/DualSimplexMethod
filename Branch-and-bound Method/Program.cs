using Branch_and_bound_Method.BranchAndBoundLibrary;
using DualSimplexMethod.ProjectUtilities;
using DualSimplexMethod.SimplexLibrary;

namespace Branch_and_bound_Method
{
    class Program
    {
        static void Main()
        {
            const string PATH = "DishesSet.csv";
            CSVToBranchAndBoundUtil util = new(PATH);
            util.ConsoleInputConstraints();

            Console.WriteLine("\nРешение на C#");
            BranchAndBound bb = new(util.BuildRightProvider());
            bb.GetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) res, new NothingOutputProvider());
            CSVUtility ut = new(PATH);
            ut.ResultInterpritate(res, new ConsoleOutputProvider());

            Console.WriteLine("\nРешение на Python");
            SimplexCSharpToPython cspt = new(util.BuildRightProvider());
            ut.ResultInterpritate(cspt.PythonSolve(), new ConsoleOutputProvider());
        }
    }
}
