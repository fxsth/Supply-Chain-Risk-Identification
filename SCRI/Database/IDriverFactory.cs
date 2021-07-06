using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Database
{
    public interface IDriverFactory
    {
        public IDriver CreateDriver();
    }
}
