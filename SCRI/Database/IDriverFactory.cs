using Neo4j.Driver;
using System;

namespace SCRI.Database
{
    public interface IDriverFactory
    {
        Action<ConfigBuilder> Action { get; set; }
        IAuthToken AuthToken { get; set; }
        string URI { get; set; }

        IDriver CreateDriver();
    }
}