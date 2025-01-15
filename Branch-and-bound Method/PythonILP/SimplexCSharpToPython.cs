using DualSimplexMethod.SimplexLibrary;
using Python.Runtime;

namespace DualSimplexMethod.ProjectUtilities
{
    public class SimplexCSharpToPython
    {
        private List<Constraint> _canonicalConstraints;
        private readonly double[] _target;
        private readonly double[] _rightVector;
        private readonly double[,] _constraintsMatrix;
        public SimplexCSharpToPython(ISimplexDataProvider simplexData)
        {
            _target = [.. simplexData.GetTargetFunctional().Coefficients.GetArray()];

            _canonicalConstraints = ConstraintsCanonicalization(simplexData.GetConstraints());
            int constraintsCount = _canonicalConstraints.Count;
            int coefCount = _canonicalConstraints[0].Coefficients.Length;

            _constraintsMatrix = new double[constraintsCount, coefCount];
            _rightVector = new double[constraintsCount];

            for (int i = 0; i < constraintsCount; i++)
            {
                _rightVector[i] = _canonicalConstraints[i].Solve;
                for (int j = 0; j < coefCount; j++)
                {
                    _constraintsMatrix[i, j] = _canonicalConstraints[i].Coefficients[j];
                }
            }
        }

        private static List<Constraint> ConstraintsCanonicalization(List<Constraint> constraints)
        {
            var result = new List<Constraint>();

            foreach (Constraint constraint in constraints)
            {
                switch (constraint.Sign)
                {
                    case InequalitySign.LessThanOrEqual:
                        result.Add(constraint);
                        break;
                    case InequalitySign.GreaterThanOrEqual:
                        result.Add(new(-constraint.Coefficients, InequalitySign.LessThanOrEqual, -constraint.Solve));
                        break;
                    case InequalitySign.Equals:
                        result.Add(new(constraint.Coefficients, InequalitySign.LessThanOrEqual, constraint.Solve));
                        result.Add(new(-constraint.Coefficients, InequalitySign.LessThanOrEqual, -constraint.Solve));
                        break;
                }
            }

            return result;
        }
        public (Dictionary<ResourceInfo, double> Solution, double Solve) GetResult()
        {
            Dictionary<ResourceInfo, double> Solution = [];
            double solve = 0;
            Runtime.PythonDLL = @"C:\Users\User\AppData\Local\Programs\Python\Python310\python310.dll";
            PythonEngine.Initialize();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append("D:\\С#\\DualSimplexMethod\\Branch-and-bound Method\\PythonILP\\");
                    var lpSolver = Py.Import("PythonILP");

                    var target = new PyList(_target.Select(item => item.ToPython()).ToArray());

                    PyList constraints = new();

                    for (int i = 0;i< _canonicalConstraints.Count; i++)
                    {
                        constraints.Append(new PyList(_canonicalConstraints[i].Coefficients.GetArray().Select(item => item.ToPython()).ToArray()));
                    }

                    var rightVector = new PyList(_rightVector.Select(item => item.ToPython()).ToArray());
                    var variablesBound = new PyTuple([0.0.ToPython(), double.PositiveInfinity.ToPython()]);

                    var result = lpSolver.InvokeMethod("solve_ilp", [target, constraints, rightVector]);
                    if (result["success"].As<int>() == 1)
                    {
                        double[] x = result["variables"].As<double[]>();

                        for (int iter = 0; iter < x.Length; iter++)
                        {
                            if (x[iter] == 0) continue;
                            Solution.Add(new(iter, $"x{iter}", VariableRole.Main), x[iter]);
                        }

                        solve = result["objective_value"].As<double>();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            RuntimeData.FormatterType = typeof(NoopFormatter);
            PythonEngine.Shutdown();

            return (Solution, solve);
        }
    }
}
