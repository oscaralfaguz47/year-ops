using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;

namespace OceansApp.DataAccess
{
    internal class DatabaseService
    {
        private readonly string _connectionString;
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _isDatabaseAvailable = true;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
            _retryPolicy = Policy
                .Handle<SqlException>(ex => ex.Number == -2 || ex.Number == 40613) // Timeout or database unavailable
                .WaitAndRetryAsync(new[]
                {
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
                });
        }

        public async Task CheckDatabaseConnectionAsync()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync();
                    _isDatabaseAvailable = true;
                }
            });
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<SqlConnection, Task<T>> operation)
        {
            if (!_isDatabaseAvailable)
            {
                await CheckDatabaseConnectionAsync();
            }

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await operation(connection);
                }
            });
        }

        public async Task ExecuteWithRetryAsync(Func<SqlConnection, Task> operation)
        {
            if (!_isDatabaseAvailable)
            {
                await CheckDatabaseConnectionAsync();
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await operation(connection);
                }
            });
        }
    }
}
