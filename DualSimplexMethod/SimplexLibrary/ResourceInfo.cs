namespace DualSimplexMethod.SimplexLibrary
{

    public record class ResourceInfo
    {
        public int Number { get; init; }
        public string Title { get; init; }
        public VariableRole Role { get; init; }
        public string Description { get; init; }
        public ResourceInfo(int number, string title, VariableRole role) : this(number, title, role, string.Empty) { }
        public ResourceInfo(int number, string title, VariableRole role, string description)
        {
            if (number < 0) throw new ArgumentException("Number should be greater or equal then 0");
            Number = number;
            Title = title;
            Role = role;
            Description = description;
        }
        public static implicit operator int(ResourceInfo res) => res.Number;
        public override string ToString()
        {
            return Title;
        }
    }
}
