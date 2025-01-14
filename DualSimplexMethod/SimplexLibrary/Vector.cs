using System;

namespace DualSimplexMethod.SimplexLibrary
{
    public class Vector
    {
        private const double exact = 1e-14;
        private double[] _source;
        public int Length => _source.Length;

        public Vector(double[] source)
        {
            if (source.Length == 0) throw new Exception("Vector should have length greater than zero");
            _source = new double[source.Length];
            Array.Copy(source, _source, source.Length);
        }
        public Vector(Vector vec) : this(vec._source) { }

        public Vector(double value, int length) : this(new double[length])
        {
            Fill(value);
        }
        public double this[int index]
        {
            get => double.Abs(_source[index]) < exact ? 0 : _source[index];
            set => _source[index] = value;
        }
        public Vector this[Range range]
        {
            get => new(_source[range]);
        }
        public static Vector operator +(Vector left, Vector right)
        {
            if (left is null || right is null) throw new NullReferenceException();
            if (left.Length != right.Length) throw new Exception("Vectors have different length");

            double sumBuffer;
            int resultLength = left.Length;
            double[] result = new double[resultLength];
            for (int iter = 0; iter < resultLength; iter++)
            {
                sumBuffer = left[iter] + right[iter];
                result[iter] = double.Abs(sumBuffer) < exact ? 0 : sumBuffer;
            }

            return new Vector(result);
        }
        public static Vector operator -(Vector left, Vector right)
        {
            if (left is null || right is null) throw new NullReferenceException();
            if (left.Length != right.Length) throw new Exception("Vectors have different length");
            double subBuffer;
            double[] result = new double[left.Length];
            for (int iter = 0; iter < left.Length; iter++)
            {
                subBuffer = left[iter] - right[iter];
                result[iter] = double.Abs(subBuffer) < exact ? 0 : subBuffer;
            }
            return new Vector(result);
        }
        public static Vector operator -(Vector self)
        {
            ArgumentNullException.ThrowIfNull(self);

            double[] result = new double[self.Length];

            for (int iter = 0; iter < self.Length; iter++) result[iter] = -self[iter];

            return new Vector(result);
        }
        public static Vector operator *(Vector left, double num)
        {
            ArgumentNullException.ThrowIfNull(left);
            double multBuffer;
            double[] result = new double[left.Length];
            for (int iter = 0; iter < left.Length; iter++)
            {
                multBuffer = left[iter] * num;
                result[iter] = double.Abs(multBuffer) < exact ? 0 : multBuffer;
            }
            return new Vector(result);
        }
        public static Vector operator /(Vector left, double num)
        {
            ArgumentNullException.ThrowIfNull(left);

            if (num == 0) throw new DivideByZeroException();

            return left * (1 / num);
        }
        public void Concat(Vector add)
        {
            _source = [.. _source, .. add._source];
        }
        public bool TrueForAll(Predicate<double> match) => Array.TrueForAll(_source, match);
        public void Fill(double value) => Array.Fill(_source, value);
        public double[] GetArray() => _source;
        public override string ToString()
        {
            return $"< {string.Join(" ", _source)} >";
        }

    }
}
