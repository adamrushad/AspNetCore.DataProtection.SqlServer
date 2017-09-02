using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace AspNetCore.DataProtection.SqlServer
{
    public static class SqlServerDataProtectionBuilderExtensions
    {
        public static IDataProtectionBuilder PersistKeysToSqlServer(this IDataProtectionBuilder builder, string connectionString, string schema = "DataProtection", string table = "Keys")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new SqlServerXmlRepository(connectionString, schema, table);
            });

            return builder;
        }
    }
}
