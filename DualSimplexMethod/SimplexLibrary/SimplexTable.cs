namespace DualSimplexMethod.SimplexLibrary
{
    class SimplexTable
    {
        private const int PADDING = 9;
        //private const double EXACT = 1e-12;
        private int VarsQuantity => _resourceVariables.Length;
        private readonly ResourceInfo[] _resourceVariables;
        private readonly ResourceInfo[] _addVars;
        public List<SimplexLine> Lines { get; }
        public Vector OptimizationLine { get; set; }
        public double TargetSolve { get; set; }

        public SimplexTable(List<Constraint> constraints, Vector functional, string[] descriptions)
        {
            int addVariables = constraints.Count;
            int variables = functional.Length - addVariables;

            if (descriptions.Length != variables + addVariables)
            {
                string[] temp = new string[variables + addVariables];
                Array.Fill(temp, string.Empty);
                descriptions.CopyTo(temp, 0);
                descriptions = temp;
            }

            if (variables <= 0 || addVariables <= 0)
            {
                throw new ArgumentException("Number of variables or constraints less then or equal to zero");
            }

            _resourceVariables = ResourceInfosBuild(variables, addVariables, descriptions);
            _addVars = [.. _resourceVariables[variables..]];
            Lines = ConvertConstraintsToSimplexLines(constraints);
            OptimizationLine = functional;
            TargetSolve = 0;
        }

        private static ResourceInfo[] ResourceInfosBuild(int variables, int addVariables, string[] descriptions)
        {
            ResourceInfo[] result = new ResourceInfo[addVariables + variables];

            for (int iter = 0; iter < result.Length; iter++)
            {
                if (iter < variables)
                    result[iter] = new(iter, $"x{iter + 1}", VariableRole.Main, descriptions[iter]);
                else
                    result[iter] = new(iter, $"s{iter - variables + 1}", VariableRole.Additional);
            }

            return result;
        }

        private List<SimplexLine> ConvertConstraintsToSimplexLines(List<Constraint> constraints)
        {
            List<SimplexLine> lines = [];

            for (int iter = 0; iter < constraints.Count; iter++)
            {
                lines.Add(new(_addVars[iter], constraints[iter].Coefficients, constraints[iter].Solve));
            }

            return lines;
        }

        public SimplexLine this[ResourceInfo fromBasic]
        {
            get => Lines.Find(line => line.Basic == fromBasic) ?? throw new ArgumentOutOfRangeException($"{fromBasic} not found");
        }

        public SimplexColumn this[ResourceInfo fromBasic, int column]
        {
            get => new(_resourceVariables[column], this[fromBasic].Coef[column], OptimizationLine[column]);
        }

        public Dictionary<ResourceInfo, double> GetCurrentSolve()
        {
            Dictionary<ResourceInfo, double> currentSolve = [];

            foreach (ResourceInfo res in _resourceVariables) currentSolve.Add(res, 0);

            foreach (SimplexLine line in Lines) currentSolve[line.Basic] = line.Solve;

            return currentSolve;
        }

        public string GetPrintString()
        {
            string result = string.Empty;
            string divLine = string.Empty.PadLeft((PADDING + 1) * (VarsQuantity + 1) + 5, '-') + "\n";
            result += "    |";

            foreach (ResourceInfo ri in _resourceVariables)
            {
                result += ri.ToString().PadLeft(PADDING) + "|";
            }

            result += "Solve".PadLeft(PADDING) + "|";
            result += "\n" + divLine;

            foreach (SimplexLine sl in Lines)
            {
                result += sl.Basic.ToString().PadLeft(4) + "|";
                foreach (double i in sl.Coef.GetArray())
                {
                    result += $"{i:f2}".PadLeft(PADDING) + "|";
                }
                result += $"{sl.Solve:f2}".PadLeft(PADDING) + "|";
                result += "\n";
                result += divLine;
            }

            result += "  Z |";

            foreach (double i in OptimizationLine.GetArray())
            {
                result += $"{i:f2}".PadLeft(PADDING) + "|";
            }

            result += $"{TargetSolve:f2}".PadLeft(PADDING) + "|";

            return result;
        }

        public bool IsOptimizeAndFeasibleSolve(OptimizationDirection direction)
        {
            bool result = true;

            foreach (SimplexLine line in Lines)
            {
                result &= line.Solve >= 0;
            }

            switch (direction)
            {
                case OptimizationDirection.Minimize:
                    result &= OptimizationLine.TrueForAll(x => x <= 0);
                    break;
                case OptimizationDirection.Maximize:
                    result &= OptimizationLine.TrueForAll(x => x >= 0);
                    break;
            }

            return result;
        }

        public void Collapse(ResourceInfo excluded, ResourceInfo input)
        {
            if (!_resourceVariables.Contains(input)) throw new ArgumentException("");

            SimplexLine collapsingLine = this[excluded];
            List<SimplexLine> reducingLines = Lines.FindAll(line => line.Basic != excluded);

            collapsingLine.Divide(collapsingLine.Coef[input]);

            Parallel.ForEach(reducingLines, line => line.Substract(collapsingLine * line.Coef[input]));

            TargetSolve -= collapsingLine.Solve * OptimizationLine[input];
            OptimizationLine -= collapsingLine.Coef * OptimizationLine[input];

            collapsingLine.Basic = input;
        }

        public bool IsFeasibleLines()
        {
            bool result = true;

            foreach (SimplexLine line in Lines)
            {
                if (line.Solve < 0) result &= !line.Coef.TrueForAll(i => i >= 0);

                if (line.Solve != 0) result &= !line.Coef.TrueForAll(i => i == 0);

                if (line.Solve > 0) result &= !line.Coef.TrueForAll(i => i <= 0);
            }
            return result;
        }
    }
}