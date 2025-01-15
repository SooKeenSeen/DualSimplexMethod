using DualSimplexMethod.ProjectUtilities;
using DualSimplexMethod.SimplexLibrary;
namespace DualSimplexMethod
{
    class Program
    {
        static void Main()
        {
            const string PATH = "DishesSet.csv";

            CSVUtility simplexConverter = new(PATH);
            simplexConverter.ConsoleInputConstraints();
            DualSimplex continuousProblemCS = new(simplexConverter.BuildDefaultProvider());

            Console.WriteLine("Решение на C#");
            continuousProblemCS.TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) resultCS);
            simplexConverter.ResultInterpritate(resultCS, new ConsoleOutputProvider());

            Console.WriteLine("\nРешение на Python");
            SimplexCSharpToPython continuousProblemPython = new(simplexConverter.BuildDefaultProvider());
            simplexConverter.ResultInterpritate(continuousProblemPython.GetResult(), new ConsoleOutputProvider());
        }
    }
}
