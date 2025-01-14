namespace DualSimplexMethod.SimplexLibrary
{
    class SimplexLine
    {
        private const double exact = 1e-12;
        public ResourceInfo Basic { get; set; }
        public Vector Coef { get; set; }
        private double _solve;
        public double Solve { get { return _solve; } set { _solve = double.Abs(value) < exact ? 0 : value; } }

        public int Length => Coef.Length;
        public SimplexLine(ResourceInfo basic, Vector coefficients, double solve)
        {
            Basic = basic;
            Coef = coefficients;
            Solve = solve;
        }
        public double this[int index]
        {
            get => Coef[index];
        }
        public void Divide(double num)
        {
            if (num == 0) return;
            Coef /= num;
            Solve /= num;
        }
        public void Substract(SimplexLine other)
        {
            ArgumentNullException.ThrowIfNull(other);
            Coef -= other.Coef;
            Solve -= other.Solve;
        }
        public static SimplexLine operator *(SimplexLine left, double right)
        {
            return new SimplexLine(left.Basic, left.Coef * right, left.Solve * right);
        }

    }
}
