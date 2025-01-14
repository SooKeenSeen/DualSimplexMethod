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
            //SimplexCSharpToPython cspt = new(util.BuildRightProvider());
            //cspt.PythonSolve();
            DualSimplex ds = new(util.BuildRightProvider());
            //ds.StepByStepSolution(new FileOutputProvider("out.txt"));

            Console.WriteLine("Решение на C#");
            ds.TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) res);
            util.ResultInterpritate(res,new ConsoleOutputProvider());
            Console.WriteLine();
            Console.WriteLine("Решение на Python");
            SimplexCSharpToPython cspt = new(util.BuildRightProvider());
            util.ResultInterpritate(cspt.PythonSolve(),new ConsoleOutputProvider());
            

            //Console.WriteLine(String.Join("\n", res.Solution.Where(x => x.Value != 0).Select(x => $"{x.Key}\t{x.Key.Description}\t{x.Value:f2}")));

            //Console.WriteLine($"\nWith SOLVE = {res.Solve:f2}");


        }
    }
}
