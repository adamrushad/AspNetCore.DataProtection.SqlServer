/*
 * Copyright (c) 2017 Adam Walters
 * Licensed under the terms of the MIT license
 * License available at: https://raw.githubusercontent.com/abwalters/AspNetCore.DataProtection.SqlServer/master/LICENSE
 */
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace AspNetCore.DataProtection.SqlServer
{
    /// <summary>
    /// Extension method for DataProtection configuration
    /// </summary>
    public static class SqlServerDataProtectionBuilderExtensions
    {
        /// <summary>
        /// Stores DataProtection subsystem keys in a Microsoft SQL database
        /// </summary>
        /// <param name="builder">DataProtection configuration builder. Automatically passed</param>
        /// <param name="connectionString">Connection string for desired SQL database</param>
        /// <param name="schema">Schema to use for the table. Defaults to "DataProtection" if not specified</param>
        /// <param name="table">Table name to use. Defaults to "Keys" if not specified</param>
        public static IDataProtectionBuilder PersistKeysToSqlServer(this IDataProtectionBuilder builder, string connectionString, string schema = "DataProtection", string table = "Keys")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

#if NETSTANDARD1_3
            builder.Services.TryAddSingleton<IXmlRepository>(services => new SqlServerXmlRepository(connectionString, schema, table));
#elif NETSTANDARD2_0
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new SqlServerXmlRepository(connectionString, schema, table);
            });
#endif
            return builder;
        }
    }
}
