using DualSimplexMethod.SimplexLibrary;
using System.Diagnostics;


namespace Branch_and_bound_Method.BranchAndBoundLibrary
{
    class BranchAndBound
    {
        private const double integerExact = 1E-8;
        private readonly DualSimplex _dualSimplex;
        private double _realInitSolve = 0;
        private readonly int _numberOfVariables;
        private readonly string[] _names;
        private readonly List<Constraint> _initialConstraints;
        private readonly TargetFunctional _initialTarget;
        private double InitWholeOptimalSolve =>
    _initialTarget.Optimization == OptimizationDirection.Minimize ? Math.Ceiling(_realInitSolve) : Math.Floor(_realInitSolve);

        public BranchAndBound(ISimplexDataProvider simplexDataProvider)
        {
            _initialConstraints = simplexDataProvider.GetConstraints();
            _initialTarget = simplexDataProvider.GetTargetFunctional();
            _dualSimplex = new(simplexDataProvider);
            _names = simplexDataProvider.GetDescriptions();
            _numberOfVariables = _initialTarget.Coefficients.Length;
        }
        private static bool IsIntegerSolution((Dictionary<ResourceInfo, double> Solution, double Solve) sol)
        {
            return sol.Solution.Where(pair => pair.Key.Role == VariableRole.Main).All(pair => IsInteger(pair.Value));
        }
        private static bool IsInteger(double num) => double.Abs(num % 1) < integerExact || 1 - double.Abs(num % 1) < integerExact;
        public bool GetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) problemSolve, IDataOutputProvider provider)
        {
            object _nonProbingLock = new();
            object _probingLock = new();
            object _boundLock = new();

            if (!_dualSimplex.TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) result))
            {
                provider.Output("Problem has no feasible solution.\n");
                problemSolve = ([],0);
                return false;
            }

            if (IsIntegerSolution(result))
            {
                provider.Output(String.Join("\n", result.Solution.Where(x => x.Value != 0).Select(x => $"{x.Key}\t{x.Key.Description}\t{x.Value:f2}")));

                provider.Output($"\nWith SOLVE = {result.Solve:f2}");
                problemSolve = result;
                return true;
            }

            _realInitSolve = result.Solve;
            double bound = _initialTarget.Optimization == OptimizationDirection.Minimize ? double.MaxValue : double.MinValue;

            List<(List<Constraint> Constraints, DualSimplex Problem)> branchesTasks = [];
            List<(List<Constraint> Constraints, (Dictionary<ResourceInfo, double> Solution, double Solve) Result)> probingTasks = [];
            List<(List<Constraint> Constraints, (Dictionary<ResourceInfo, double> Solution, double Solve) Result)> nonProbingTasks = [];

            nonProbingTasks.Add((_initialConstraints, result));

            Stopwatch stopwatch = new();
            stopwatch.Start();

            while (true)
            {
                Parallel.ForEach(nonProbingTasks, task =>
                {
                    Vector v = new(0, _numberOfVariables);
                    (ResourceInfo nonIntegerResource, double value) = task.Result.Solution.FirstOrDefault(x => !IsInteger(x.Value) && x.Key.Role == VariableRole.Main);
                    v[nonIntegerResource] = 1;

                    List<Constraint> lessBranch = [.. task.Constraints];
                    List<Constraint> greaterBranch = [.. task.Constraints];

                    lessBranch.Add(new Constraint(v, InequalitySign.LessThanOrEqual, Math.Floor(value)));
                    greaterBranch.Add(new Constraint(v, InequalitySign.GreaterThanOrEqual, Math.Ceiling(value)));

                    branchesTasks.Add((lessBranch, new(new DefaultSimplexDataProvider(lessBranch, _initialTarget, _names))));
                    branchesTasks.Add((greaterBranch, new(new DefaultSimplexDataProvider(greaterBranch, _initialTarget, _names))));
                });

                nonProbingTasks.Clear();

                Parallel.ForEach(branchesTasks, task =>
                {
                    if (!task.Problem.TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) branchTaskResult)) return;

                    if (IsIntegerSolution(branchTaskResult))
                    {
                        lock (_boundLock)
                        {
                            switch (_initialTarget.Optimization)
                            {
                                case OptimizationDirection.Maximize:
                                    if (branchTaskResult.Solve > bound)
                                        bound = branchTaskResult.Solve;
                                    break;
                                case OptimizationDirection.Minimize:
                                    if (branchTaskResult.Solve < bound)
                                        bound = branchTaskResult.Solve;
                                    break;
                            }
                        }
                        lock (_probingLock)
                        {
                            probingTasks.Add((task.Constraints, branchTaskResult));
                        }
                    }
                    else
                    {
                        lock (_nonProbingLock)
                        {
                            nonProbingTasks.Add((task.Constraints, branchTaskResult));
                        }
                    }
                });

                nonProbingTasks.RemoveAll(t => _initialTarget.Optimization == OptimizationDirection.Minimize ? t.Result.Solve > bound : t.Result.Solve < bound);
                branchesTasks.Clear();

                if (!probingTasks.TrueForAll(task => task.Result.Solve != InitWholeOptimalSolve)) break;
                if (nonProbingTasks.Count == 0) break;
            }

            stopwatch.Stop();

            if (probingTasks.Count == 0) { Console.WriteLine("Has no feasible integer solution."); problemSolve = ([], 0); return false; }

            var res = _initialTarget.Optimization switch
            {
                OptimizationDirection.Maximize => probingTasks.MaxBy(task => task.Result.Solve).Result,
                OptimizationDirection.Minimize => probingTasks.MinBy(task => task.Result.Solve).Result,
                _ => ([], 0),
            };

            problemSolve = res;

            provider.Output(String.Join("\n", res.Solution.Where(x => x.Value != 0 && x.Key.Role == VariableRole.Main).Select(x => $"{x.Key}\t{x.Key.Description}\t{x.Value:f2}")));

            provider.Output($"\nWith Integer SOLVE = {res.Solve:f2}\n");

            provider.Output($"Continuous SOLVE = {_realInitSolve:f2}\n");

            provider.Output($"TIME = {stopwatch.ElapsedMilliseconds}\n");

            return true;
        }
    }
}
