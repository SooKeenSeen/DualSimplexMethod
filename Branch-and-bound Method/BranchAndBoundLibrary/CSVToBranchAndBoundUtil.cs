using DualSimplexMethod.SimplexLibrary;
using System.Text;

namespace Branch_and_bound_Method.BranchAndBoundLibrary
{
    class CSVToBranchAndBoundUtil
    {
        private const int dishesMaxCount = 2;
        private double _delta;
        private readonly string[] _variablesNames;
        private readonly List<List<string>> _columnView;
        private readonly List<string[]> _linesView;
        private readonly List<ConstraintDTO> _constraintsVectors;
        private readonly Vector _targetVector;

        public CSVToBranchAndBoundUtil(string path)
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
            Console.WriteLine("Enter delta as a percentage:");

            _delta = Convert.ToDouble(Console.ReadLine());

            if (_delta < 0 || _delta > 100) throw new Exception("Delta must be between 0 and 100!!!");

            _delta /= 100;

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
                lc.Add(new(constraint.Values, InequalitySign.GreaterThanOrEqual, constraint.Limit - constraint.Limit * _delta));
                lc.Add(new(constraint.Values, InequalitySign.LessThanOrEqual, constraint.Limit + constraint.Limit * _delta));
            }

            for (int i = 0; i < _targetVector.Length; i++)
            {
                Vector v = new(0, _targetVector.Length);
                v[i] = 1;
                lc.Add(new(v, InequalitySign.LessThanOrEqual, dishesMaxCount));
            }

            TargetFunctional tf = new(_targetVector, OptimizationDirection.Minimize);

            return new(lc, tf, _variablesNames);
        }
    }
}
