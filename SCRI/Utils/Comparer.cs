using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    public class SupplierComparer : IEqualityComparer<Models.Supplier>
    {
        public bool Equals(Supplier x, Supplier y) => x.ID == y.ID;

        public int GetHashCode([DisallowNull] Supplier obj) => obj.GetHashCode();
    }

    public class SupplierRelationshipComparer : IEqualityComparer<SupplierRelationship>
    {
        public bool Equals(SupplierRelationship x, SupplierRelationship y) => x.ID == y.ID;

        public int GetHashCode([DisallowNull] SupplierRelationship obj) => obj.GetHashCode();
    }
}
