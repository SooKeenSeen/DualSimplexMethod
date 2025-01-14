using DualSimplexMethod.SimplexLibrary;
using System.Text;


namespace DualSimplexMethod.ProjectUtilities
{
    public class CSVUtility
    {
        private const int NAME_PADDING = 35;
        private const int VALUE_PADDING = 15;
        private const int X_PADDING = 6;
        private double _delta;
        private readonly string[] _variablesNames;
        private readonly List<List<string>> _columnView;
        private readonly List<string[]> _linesView;
        private readonly List<ConstraintDTO> _constraintsVectors;
        private readonly Vector _targetVector;

        public CSVUtility(string path)
        {
            //For correct reading сyrillic sign
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Read CSV to List<string[]> for lines
            StreamReader csvReader = new(path, Encoding.GetEncoding(1251));

            _linesView = [];

            string BufferStr = string.Empty;

            while (!csvReader.EndOfStream)
            {
                BufferStr = csvReader.ReadLine() ?? string.Empty;
                _linesView.Add(BufferStr.Split(';'));
            }

            csvReader.Close();

            int lineLength = _linesView[0].Length;
            if (!_linesView.TrueForAll(line => line.Length == lineLength)) throw new InvalidDataException();

            //Convert List<string[]> linesView to List<List<string>> columnView where List<string> like column
            _columnView = [];

            int length = _linesView.Count;
            int width = _linesView[0].Length;

            for (int i = 0; i < width; i++) _columnView.Add([]);

            for (int line = 0; line < length; line++)
            {
                for (int i = 0; i < width; i++) _columnView[i].Add(_linesView[line][i]);
            }

            //Prepare and cast data to RightProvieder
            _variablesNames = [.. _columnView[0][1..]];
            _targetVector = new(_columnView[1][1..].Select(val => double.Parse(val)).ToArray());
            _constraintsVectors = [];

            for (int i = 2; i < _columnView.Count; i++)
            {
                _constraintsVectors.Add(new(_columnView[i][0], new(_columnView[i][1..].Select(val => double.Parse(val)).ToArray()), 0));
            }
        }
        public void ConsoleInputConstraints()
        {
            Console.WriteLine("Enter delta");

            _delta = Convert.ToDouble(Console.ReadLine());

            for (int i = 0; i < _constraintsVectors.Count; i++)
            {
                Console.WriteLine($"Put a limit on {_constraintsVectors[i].ColumnName}");
                _constraintsVectors[i].Limit = Convert.ToDouble(Console.ReadLine());
            }
        }
        public DefaultSimplexDataProvider BuildRightProvider()
        {
            List<Constraint> lc = [];

            foreach (var constraint in _constraintsVectors)
            {
                lc.Add(new(constraint.Values, InequalitySign.LessThanOrEqual, constraint.Limit + _delta));
                lc.Add(new(constraint.Values, InequalitySign.GreaterThanOrEqual, constraint.Limit - _delta));
            }

            TargetFunctional tf = new(_targetVector, OptimizationDirection.Minimize);

            return new(lc, tf, _variablesNames);
        }
        public void ResultInterpritate((Dictionary<ResourceInfo, double> Solution, double Solve) result, IDataOutputProvider provider)
        {
            if (result.Solution.Count == 0) { Console.WriteLine("Problem has no feasible solution"); return; }

            string divLine = string.Empty.PadLeft(NAME_PADDING + 4 + X_PADDING + (_linesView[0].Length - 1) * VALUE_PADDING, '-') + "\n";
            string output = string.Empty;
            output += _linesView[0][0].PadRight(NAME_PADDING) + "|";
            output += "x".PadLeft(X_PADDING) + "|";
            output += _linesView[0][1].PadLeft(VALUE_PADDING) + "|";

            for (int i = 2; i < _linesView[0].Length; i++)
            {
                output += _linesView[0][i].PadLeft(VALUE_PADDING);
            }

            output += "|\n";
            output += divLine;
            double[] summary = new double[_linesView[0].Length - 2];

            foreach (var pare in result.Solution)
            {
                if (pare.Key.Role == VariableRole.Additional || pare.Value == 0) continue;

                int num = pare.Key.Number + 1;
                double coef = pare.Value;

                output += _linesView[num][0].PadRight(NAME_PADDING) + "|";
                output += $"{coef:f2}".PadLeft(X_PADDING) + "|";
                output += $"{Convert.ToDouble(_linesView[num][1]) * coef:f2}".PadLeft(VALUE_PADDING) + "|";
                double buffer = 0;
                for (int iter = 2; iter < _linesView[num].Length; iter++)
                {
                    buffer = Convert.ToDouble(_linesView[num][iter]);
                    summary[iter - 2] += buffer * coef;
                    output += $"{buffer * coef:f2}".PadLeft(VALUE_PADDING);

                }

                output += "|\n";
            }

            output += divLine;

            output += "SUMMARY".PadRight(NAME_PADDING + X_PADDING + 1) + "|";
            output += $"{result.Solve:f2}".PadLeft(VALUE_PADDING) + "|";

            foreach (double val in summary)
            {
                output += $"{val:f2}".PadLeft(VALUE_PADDING);
            }
            output += "|\n";

            provider.Output(output);
        }
    }

}


