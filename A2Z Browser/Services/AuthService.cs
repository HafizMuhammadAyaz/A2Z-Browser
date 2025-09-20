using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace A2Z_Browser.Services
{
    public class AuthService
    {
        private readonly DbAccess _dbAccess;

        public AuthService(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        public async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                // Use parameterized query to prevent SQL injection
                var query = "SELECT PasswordHash FROM Users WHERE Username = @Username AND IsActive = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username)
                };

                var result = await _dbAccess.ExecuteScalarAsync(query, parameters);

                if (result != null)
                {
                    // In a real app, you would hash the input password and compare
                    // For now, we'll do a simple comparison (not secure for production)
                    var storedHash = result.ToString();
                    return storedHash == password;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserLastLogin(string username)
        {
            try
            {
                var query = "UPDATE Users SET LastLogin = GETDATE() WHERE Username = @Username";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username)
                };

                var result = await _dbAccess.ExecuteQueryAsync(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update last login error: {ex.Message}");
                return false;
            }
        }
    }
}