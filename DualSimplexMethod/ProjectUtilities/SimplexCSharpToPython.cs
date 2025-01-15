using DualSimplexMethod.SimplexLibrary;
using Python.Runtime;

namespace DualSimplexMethod.ProjectUtilities
{
    class SimplexCSharpToPython
    {
        private readonly double[] _target;
        private readonly double[] _rightVector;
        private readonly double[,] _constraintsMatrix;
        public SimplexCSharpToPython(ISimplexDataProvider simplexData)
        {
            _target = [.. simplexData.GetTargetFunctional().Coefficients.GetArray()];

            var constraints = ConstraintsCanonicalization(simplexData.GetConstraints());
            int constraintsCount = constraints.Count;
            int coefCount = constraints[0].Coefficients.Length;

            _constraintsMatrix = new double[constraintsCount, coefCount];
            _rightVector = new double[constraintsCount];

            for (int i = 0; i < constraintsCount; i++)
            {
                _rightVector[i] = constraints[i].Solve;
                for (int j = 0; j < coefCount; j++)
                {
                    _constraintsMatrix[i, j] = constraints[i].Coefficients[j];
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
        public (Dictionary<ResourceInfo, double> Solution,double Solve) GetResult()
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
                    sys.path.append("D:\\С#\\DualSimplexMethod\\DualSimplexMethod\\ProjectUtilities\\");
                    var lpSolver = Py.Import("PythonSimplex");

                    var target = _target.ToPython();
                    var constraints = _constraintsMatrix.ToPython();
                    var rightVector = _rightVector.ToPython();
                    var variablesBound = new PyTuple([0.0.ToPython(), double.PositiveInfinity.ToPython()]);

                    var result = lpSolver.InvokeMethod("solve_lp", [target, constraints, rightVector, variablesBound]);
                    if (result["success"].As<bool>())
                    {
                        double[] x = result["x"].As<double[]>();

                        for (int iter = 0; iter < x.Length; iter++)
                        {
                            if (x[iter] == 0) continue;
                            Solution.Add(new(iter, $"x{iter}", VariableRole.Main), x[iter]);
                        }

                        solve = -result["objective_value"].As<double>();
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
