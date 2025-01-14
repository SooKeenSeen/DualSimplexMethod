namespace DualSimplexMethod.SimplexLibrary
{
    public record class Constraint
    {
        public Vector Coefficients { get; init; }
        public InequalitySign Sign { get; init; }
        public double Solve { get; init; }
        public string Description { get; init; }
        public Constraint(Vector Coefficients, InequalitySign Sign, double Solve, string description)
        {
            this.Coefficients = Coefficients;
            this.Sign = Sign;
            this.Solve = Solve;
            this.Description = description;
        }
        public Constraint(Vector Coefficients, InequalitySign Sign, double Solve) : this(Coefficients, Sign, Solve, string.Empty) { }

    }

    public record class TargetFunctional(Vector Coefficients, OptimizationDirection Optimization);

    public record class SimplexColumn(ResourceInfo ColumnInfo, double Value, double Solve);

    public enum InequalitySign
    {
        GreaterThanOrEqual,
        LessThanOrEqual,
        Equals
    }
    public enum OptimizationDirection
    {
        Minimize,
        Maximize
    }
    public enum VariableRole
    {
        Main,
        Additional
    }
}
