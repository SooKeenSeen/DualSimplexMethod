using System.Linq;

namespace ParallelTest
{
    internal class Program
    {
        class A(double value)
        {
            public double Value { get; set; } = value;
            public bool Calculate(out double result)
            {
                if (Value < 0) { result = 0; return false; }
                else { result = double.Sqrt(Value); return true; }
            }
            public override string ToString()
            {
                return Value.ToString();
            }
        }
        static void Main()
        {
            object _lock = new();
            List<A> test = [new(4), new(9), new(25), new(64)];
            List<double> results = [];
            //test.AsParallel().Select(t => { t.Calculate(out double res); return res; }).ForAll(r => results.Add(r));
            Parallel.ForEach(test, t => { t.Calculate(out double res); lock (_lock) { results.Add(res); } });

            Console.WriteLine(string.Join("\t", test));
            Console.WriteLine(string.Join("\t", results));
        }
    }
}
