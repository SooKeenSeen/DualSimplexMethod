using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualSimplexMethod.SimplexLibrary;


namespace DualSimplexMethod.ProjectUtilities
{
    class ConstraintDTO(string ColumnName, Vector Values, double Limit)
    {
        public string ColumnName { get; set; } = ColumnName;
        public Vector Values { get; set; } = Values;
        public double Limit { get; set; } = Limit;
    }
}
