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

            CSVToBranchAndBoundUtil simplexConverter = new(PATH);
            CSVUtility resultInterpreter = new(PATH);
            simplexConverter.ConsoleInputConstraints();

            Console.WriteLine("\nРешение на C#");
            BranchAndBound integerProblemCS = new(simplexConverter.BuildDefaultProvider());
            integerProblemCS.GetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) resultCS, new NothingOutputProvider());
            resultInterpreter.ResultInterpritate(resultCS, new ConsoleOutputProvider());

            Console.WriteLine("\nРешение на Python");
            SimplexCSharpToPython integerProblemPython = new(simplexConverter.BuildDefaultProvider());
            resultInterpreter.ResultInterpritate(integerProblemPython.GetResult(), new ConsoleOutputProvider());
        }
    }
}
