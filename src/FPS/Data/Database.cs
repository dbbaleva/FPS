using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FPS.Data
{
    public class Database : IDisposable
    {
        private const int DefaultCommandTimeout = 30;
        private readonly Dictionary<Type, object> _converters;


        public Database(string connectionString, ConnectionType connectionType)
        {
            Debug.Assert(connectionString != null);
            switch (connectionType)
            {
                case ConnectionType.OleDbConnection:
                    Connection = new OleDbConnection(connectionString);
                    break;
                case ConnectionType.SqlConnection:
                    Connection = new SqlConnection(connectionString);
                    break;
            }

            _converters = new Dictionary<Type, object>();

            EnsureConnectionOpen();
        }

        public DbConnection Connection { get; private set; }

        public int CommandTimeout { get; set; } = DefaultCommandTimeout;

        public bool Connected => Connection != null && Connection.State == ConnectionState.Open;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Execute(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));
            EnsureConnectionOpen();
            var command = Connection.CreateCommand();
            command.CommandText = commandText;
            AddParameters(command, args);
            using (command)
                return command.ExecuteNonQuery();
        }

        public IEnumerable<Dictionary<string, object>> Query(string commandText, params object[] parameters)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));
            return QueryInternal(commandText, parameters)
                .Select(r => r.ToDictionary(q => q.Key, q => q.Value))
                .ToList();
        }

        public IEnumerable<T> Query<T>(string commandText, params object[] parameters)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));
            return QueryInternal<T>(commandText, parameters).ToArray();
        }

        public object QuerySingle(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));
            return QueryInternal(commandText, args).FirstOrDefault();
        }

        public object QueryValue(string commandText, params object[] parameters)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));

            EnsureConnectionOpen();
            var command = Connection.CreateCommand();
            command.CommandText = commandText;
            AddParameters(command, parameters);
            using (command)
                return command.ExecuteScalar();
        }

        public void SetConverter<T>(IEntityConverter<T> converter)
        {
            if (_converters.ContainsKey(typeof(T)))
                throw new ArgumentOutOfRangeException(nameof(converter));

            _converters[typeof(T)] = converter;
        }

        public void Close()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || Connection == null)
                return;
            Connection.Close();
            Connection = null;
        }

        private static void AddParameters(DbCommand command, object[] args)
        {
            if (args == null)
                return;
            foreach (var dbParameter in args.Select((o, index) =>
            {
                var local0 = command.CreateParameter();
                local0.ParameterName = index.ToString(CultureInfo.InvariantCulture);
                local0.Value = o ?? DBNull.Value;
                return local0;
            }))
                command.Parameters.Add(dbParameter);
        }

        private static IEnumerable<KeyValuePair<string, object>> PopulateRowColumns(DbDataRecord record)
        {
            for (var i = 0; i < record.FieldCount; ++i)
                yield return new KeyValuePair<string, object>(record.GetName(i), record.GetValue(i));
        }

        private void EnsureConnectionOpen()
        {
            if (Connection.State == ConnectionState.Open)
                return;
            Connection.Open();
        }

        private IEnumerable<IEnumerable<KeyValuePair<string, object>>> QueryInternal(string commandText,
            params object[] parameters)
        {
            EnsureConnectionOpen();
            var command = Connection.CreateCommand();
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            AddParameters(command, parameters);
            using (command)
            {
                using (var reader = command.ExecuteReader())
                {
                    foreach (DbDataRecord record in reader)
                        yield return PopulateRowColumns(record);
                }
            }
        }

        private IEnumerable<T> QueryInternal<T>(string commandText, params object[] parameters)
        {
            EnsureConnectionOpen();
            var command = Connection.CreateCommand();
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            AddParameters(command, parameters);
            var c = (IEntityConverter<T>)_converters[typeof(T)];
            using (command)
            {
                using (var dbDataReader = command.ExecuteReader())
                {
                    foreach (DbDataRecord record in dbDataReader)
                        yield return c.ConvertToEntity(record);
                }
            }
        }
    }
}
