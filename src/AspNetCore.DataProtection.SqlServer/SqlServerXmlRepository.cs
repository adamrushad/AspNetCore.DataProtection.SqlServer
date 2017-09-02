using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AspNetCore.DataProtection.SqlServer
{
    public class SqlServerXmlRepository : IXmlRepository
    {
        private string _connStr;
        private readonly string _schema;
        private readonly string _table;

        private static readonly Regex _regex = new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$");

        private static bool ValidateSqlIdentifier(string identifierName)
        {
            return _regex.IsMatch(identifierName);
        }

        public SqlServerXmlRepository(string connectionString, string schema = "DataProtection", string table = "Keys")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(schema))
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (!ValidateSqlIdentifier(schema))
            {
                throw new ArgumentException($"Provided schema name: '{schema}' is invalid.", nameof(schema));
            }

            if (!ValidateSqlIdentifier(table))
            {
                throw new ArgumentException($"Provided table name: '{table}' is invalid.", nameof(table));
            }

            _connStr = connectionString;
            _schema = schema;
            _table = table;

            using (SqlConnection _conn = new SqlConnection(_connStr))
            {
                _conn.Open();
                using (SqlTransaction trx = _conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    using (SqlCommand cmd = new SqlCommand($@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema)
EXEC('CREATE SCHEMA [{_schema}] AUTHORIZATION [dbo]')
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table)
CREATE TABLE [{_schema}].[{_table}](FriendlyName NVARCHAR(449) NOT NULL, XmlData NVARCHAR(MAX) NOT NULL, CONSTRAINT pk_FriendlyName PRIMARY KEY ([FriendlyName]))", _conn, trx))
                    {
                        //throw new Exception(cmd.CommandText);
                        cmd.Parameters.Add("@schema", System.Data.SqlDbType.NVarChar).Value = schema;
                        cmd.Parameters.Add("@table", System.Data.SqlDbType.NVarChar).Value = table;
                        cmd.ExecuteNonQuery();
                    }
                    trx.Commit();
                }
            }
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            List<XElement> list = new List<XElement>();

            using (SqlConnection _conn = new SqlConnection(_connStr))
            {
                _conn.Open();
                using (SqlCommand cmd = new SqlCommand($@"SELECT [XmlData] FROM [{_schema}].[{_table}]", _conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(XElement.Parse(reader.GetString(0)));
                        }
                    }
                }
            }
            return new ReadOnlyCollection<XElement>(list);
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            using (SqlConnection _conn = new SqlConnection(_connStr))
            {
                _conn.Open();
                using (SqlTransaction trx = _conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    using (SqlCommand cmd = new SqlCommand($@"IF EXISTS (SELECT * FROM [{_schema}].[{_table}] WHERE [FriendlyName] = @friendlyName)
  UPDATE [{_schema}].[{_table}] SET [XmlData] = @xmlData WHERE [FriendlyName] = @friendlyName
ELSE
  INSERT INTO [{_schema}].[{_table}] ([FriendlyName], [XmlData]) VALUES(@friendlyName, @xmlData)", _conn, trx))
                    {
                        cmd.Parameters.Add("@friendlyName", System.Data.SqlDbType.NVarChar).Value = friendlyName;
                        cmd.Parameters.Add("@xmlData", System.Data.SqlDbType.NVarChar).Value = element.ToString();
                        cmd.ExecuteNonQuery();
                    }
                    trx.Commit();
                }
            }
        }
    }
}