namespace DualSimplexMethod.SimplexLibrary
{
    public class DualSimplex
    {
        //private static object _lock = new();
        //private static int _fileNumber = 0;
        private readonly ISimplexDataProvider _dataProvider;
        private readonly OptimizationDirection optimizationDirection;
        private readonly SimplexTable _table;
        private readonly List<Constraint> _constraints;
        private readonly List<Constraint> _canonicalСonstraints = [];
        private readonly TargetFunctional _target;
        private readonly Vector _canonicalTarget;
        private int ConstraintsNum => _canonicalСonstraints.Count;
        public DualSimplex(ISimplexDataProvider dataProvider)
        {
            try
            {
                _dataProvider = dataProvider;

                Validation(_dataProvider);

                _target = _dataProvider.GetTargetFunctional();
                _constraints = _dataProvider.GetConstraints();

                CanonicalisationConstraints();
                _canonicalTarget = -_target.Coefficients;

                if (!MethodCompliance()) throw new Exception("Initial conditions does not compliance for DualSimplex method");

                AdditionalVariables();

                _table = new(_canonicalСonstraints, _canonicalTarget, _dataProvider.GetDescriptions());
                optimizationDirection = _target.Optimization;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not be constructed DualSimplex because: {ex.Message}");
            }
        }
        private static void Validation(ISimplexDataProvider dataProvider)
        {
            if (dataProvider is null) throw new NullReferenceException("DataProvider is null");

            var constraints = dataProvider.GetConstraints();
            var targetCoefs = dataProvider.GetTargetFunctional().Coefficients;
            int numberOfVariables = targetCoefs.Length;

            if (constraints is null) throw new NullReferenceException("Constraints List is null");

            if (constraints.Count == 0) throw new Exception("Constraints are missing");

            if (!constraints.TrueForAll(line => line.Coefficients.Length == numberOfVariables))
            {
                throw new Exception("The number of coefficients at variables in the target function " +
                    "does not correspond to the number of coefficients at variables in the constraints");
            }
        }
        private void CanonicalisationConstraints()
        {
            foreach (Constraint constreint in _constraints)
            {
                switch (constreint.Sign)
                {
                    case InequalitySign.Equals:
                        _canonicalСonstraints.Add(new Constraint(-constreint.Coefficients, InequalitySign.LessThanOrEqual, -constreint.Solve));
                        _canonicalСonstraints.Add(new Constraint(new(constreint.Coefficients), InequalitySign.LessThanOrEqual, constreint.Solve));
                        break;
                    case InequalitySign.GreaterThanOrEqual:
                        _canonicalСonstraints.Add(new Constraint(-constreint.Coefficients, InequalitySign.LessThanOrEqual, -constreint.Solve));
                        break;
                    case InequalitySign.LessThanOrEqual:
                        _canonicalСonstraints.Add(new Constraint(new(constreint.Coefficients), InequalitySign.LessThanOrEqual, constreint.Solve));
                        break;
                }
            }
        }
        private bool MethodCompliance()
        {
            bool isInvalidSolution = !_canonicalСonstraints.TrueForAll(con => con.Solve >= 0);
            bool isOptimizeTarget = false;
            switch (_target.Optimization)
            {
                case OptimizationDirection.Maximize:
                    isOptimizeTarget = _canonicalTarget.TrueForAll(item => item >= 0);
                    break;
                case OptimizationDirection.Minimize:
                    isOptimizeTarget = _canonicalTarget.TrueForAll(item => item <= 0);
                    break;
            }
            return isInvalidSolution && isOptimizeTarget;
        }
        private void AdditionalVariables()
        {
            Vector sample = new(0, ConstraintsNum);

            for (int i = 0; i < ConstraintsNum; i++)
            {
                sample[i] = 1;
                _canonicalСonstraints[i].Coefficients.Concat(sample);
                sample[i] = 0;
            }

            _canonicalTarget.Concat(sample);
        }
        /// <summary>Пытается получить результат вычислений без явного вывода этапов решения в виде кортежа из <c>Dictionary</c>,
        /// где ключи - объекты <c>ResourceInfo</c>, которые содержат информацию о переменных задачи,
        /// и <c>double</c> Solve - значения функционала в точке минимума/максимума. 
        /// В случае недопустимости решения, аргументу <c>result</c> будет присвоен
        /// кортеж, в котором <c>Solution</c> будет пустым, а <c>Solve</c> равен 0. 
        /// </summary>
        /// <returns>
        /// <code>true</code> в случае удачной попытки получить решение
        /// <code>false</code> в случае, если решение недопустимо.
        /// </returns>
        public bool TryGetResult(out (Dictionary<ResourceInfo, double> Solution, double Solve) result)
        {
            while (!_table.IsOptimizeAndFeasibleSolve(optimizationDirection))
            {
                if (!_table.IsFeasibleLines()) { result = ([], 0); return false; }

                NextStep();
            }

            //StepByStepSolution(new FileOutputProvider($"res{_fileNumber}.txt"));
            //lock (_lock) { _fileNumber++; }
            //if (!_table.IsFeasibleLines()) { result = ([], 0); return false; }

            result = (_table.GetCurrentSolve(), _table.TargetSolve);

            return true;
        }
        /// <summary>
        /// Предоставляет этапы решения используемому <typeparamref name="IDataOutputProvider"/> <paramref name="provider"/> в виде таблиц представленных через тип <c>string</c>, в том числе
        /// предоставляя информацию о вводимых и исключаемых переменных, а также векторе решеия и его значения.
        /// </summary>    
        public void StepByStepSolution(IDataOutputProvider provider)
        {
            provider.Output(_table.GetPrintString() + "\n\n");

            while (!_table.IsOptimizeAndFeasibleSolve(optimizationDirection))
            {
                if (!_table.IsFeasibleLines()) { provider.Output("Problem has not feasible solution"); return; }

                var (excluded, input) = NextStep();

                provider.Output($"Excluded: {excluded}\tInput: {input}\n\n" + _table.GetPrintString() + "\n\n");
            }

            var solve = _table.GetCurrentSolve();

            provider.Output(String.Join("\n", solve.Where(x => x.Value != 0).Select(x => $"{x.Key}\t{x.Key.Description}\t{x.Value:f2}")));

            provider.Output($"\nWith SOLVE = {_table.TargetSolve:f2}");
        }

        private (ResourceInfo Excluded, ResourceInfo Input) NextStep()
        {
            SimplexLine smallestLine = _table.Lines.MinBy(line => line.Solve) ?? throw new Exception();
            Vector target = _table.OptimizationLine;

            ResourceInfo excluded = smallestLine.Basic;
            ResourceInfo input = FindInput(smallestLine) ?? throw new Exception("Could not find the input variable");

            _table.Collapse(excluded, input);

            return (excluded, input);
        }
        private ResourceInfo? FindInput(SimplexLine line)
        {
            double relation = double.MaxValue;
            ResourceInfo? inputResult = null;

            for (int iter = 0; iter < line.Coef.Length; iter++)
            {
                if (line.Coef[iter] >= 0) continue;

                SimplexColumn currColumn = _table[line.Basic, iter];

                if (double.Abs(currColumn.Solve / currColumn.Value) < relation)
                {
                    relation = double.Abs(currColumn.Solve / currColumn.Value);
                    inputResult = currColumn.ColumnInfo;
                }
            }

            return inputResult;
        }
    }
}