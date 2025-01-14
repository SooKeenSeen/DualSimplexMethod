using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplexMethod.SimplexLibrary
{
    public class DefaultSimplexDataProvider(List<Constraint> constraints, TargetFunctional targetFunctional, string[] descriptions) : ISimplexDataProvider
    {
        private readonly List<Constraint> _constraints = constraints;
        private readonly TargetFunctional _targetFunctional = targetFunctional;
        private readonly string[] _descriptions = descriptions;

        public DefaultSimplexDataProvider(List<Constraint> constraints, TargetFunctional targetFunctional) : this(constraints, targetFunctional, []) { }

        public List<Constraint> GetConstraints()
        {
            return _constraints;
        }
        public string[] GetDescriptions()
        {
            return _descriptions;
        }
        public TargetFunctional GetTargetFunctional()
        {
            return _targetFunctional;
        }
    }
}
