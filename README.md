# AspNetCore.DataProtection.SqlServer
## SQL Server backend for DataProtection keys

### Example usage

While this example shows the full configuration format, do note that the schema and table names are completely optional. If not specified, the default schema name used will be DataProtection, and the default table name will be Keys. Additionally, the below example does not configure any key encryption. In a production setup, you should absolutely be encrypting the keys.

```cs
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.DataProtection.SqlServer;

namespace Example
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .PersistKeysToSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Example;Trusted_Connection=True;", "SchemaName", "TableName");
        }
    }
}
```

### Notes

* To protect against SQL injection, I utilize parameterized queries.
  * The schema and table can't be parameterized (unfortunately), but I do data validation on both before using them.
* This package is compatible with almost any version of Microsoft SQL as the storage backend.
* I avoided use of the MERGE statement, which would require SQL 2008 or above.
* I utilize the SERIALIZABLE transaction isolation level in order to ensure consistency.
  * This is to protect in the very unlikely edge case that two machines generate differnt keys with the same FriendlyName simultaneously.