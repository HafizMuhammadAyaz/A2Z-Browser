using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace A2Z_Browser.Services
{
    public class DbAccess : IDisposable
    {
        private SqlConnection _connection;
        private bool _disposed = false;
        private readonly string _connectionString;

        public DbAccess(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
        }

        // Ensure connection is open
        private void EnsureConnected()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        // Ensure async connection is open
        private async Task EnsureConnectedAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        //=== Fetching Data ===//

        /// <summary>
        /// Retrieves a DataTable based on the query provided.
        /// </summary>
        public DataTable SelectDataTable(string query, SqlParameter[]? parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                EnsureConnected();
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        dt.Load(rdr);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                // Include the original exception message and inner exceptions
                string errorMessage = $"SelectDataTable failed: {GetFullExceptionMessage(ex)}";
                throw new Exception(errorMessage, ex);
            }
        }

        // Helper method to get all exception messages
        private string GetFullExceptionMessage(Exception ex)
        {
            if (ex == null)
                return string.Empty;

            string message = ex.Message;
            Exception? innerEx = ex.InnerException;

            while (innerEx != null)
            {
                message += $"\nInner Exception: {innerEx.Message}";
                innerEx = innerEx.InnerException;
            }

            return message;
        }

        /// <summary>
        /// Retrieves a DataTable asynchronously.
        /// </summary>
        public async Task<DataTable> SelectDataTableAsync(string query, SqlParameter[]? parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                await EnsureConnectedAsync();
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(rdr);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("SelectDataTableAsync failed", ex);
            }
        }

        /// <summary>
        /// Retrieves a single scalar value from the database.
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string query, SqlParameter[]? parameters = null)
        {
            try
            {
                await EnsureConnectedAsync();
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return await cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteScalarAsync failed", ex);
            }
        }

        /// <summary>
        /// Executes an insert or update query asynchronously.
        /// </summary>
        public async Task<int> ExecuteQueryAsync(string query, SqlParameter[]? parameters = null)
        {
            try
            {
                await EnsureConnectedAsync();
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteQueryAsync failed", ex);
            }
        }

        //=== Disposable Pattern ===//
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _connection != null)
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                }
                _disposed = true;
            }
        }

        ~DbAccess()
        {
            Dispose(false);
        }
    }
}