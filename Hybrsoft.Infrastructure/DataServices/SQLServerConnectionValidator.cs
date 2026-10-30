using Hybrsoft.Enums;
using Microsoft.Data.SqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hybrsoft.Infrastructure.DataServices
{
	public sealed record DatabaseConnectionTestResult(
		bool Succeeded,
		DatabaseConnectionFailureReason FailureReason,
		string TechnicalMessage)
	{
		public static DatabaseConnectionTestResult Success() =>
			new(true, DatabaseConnectionFailureReason.None, string.Empty);
		public static DatabaseConnectionTestResult Failure(DatabaseConnectionFailureReason failureReason, string technicalMessage = "") =>
			new(false, failureReason, technicalMessage);
	}

	public sealed record SQLServerConnectionOptions(
		string Server,
		string Database,
		bool UsesWindowsAuthentication,
		string UserName,
		string Password,
		bool TrustServerCertificate,
		string ApplicationName);

	public static class SQLServerConnectionValidator
	{
		public static string CreateConnectionString(SQLServerConnectionOptions options)
		{
			return new SqlConnectionStringBuilder
			{
				DataSource = options.Server,
				InitialCatalog = options.Database,
				IntegratedSecurity = options.UsesWindowsAuthentication,
				UserID = options.UsesWindowsAuthentication ? string.Empty : options.UserName,
				Password = options.UsesWindowsAuthentication ? string.Empty : options.Password,
				Encrypt = true,
				TrustServerCertificate = options.TrustServerCertificate,
				PersistSecurityInfo = false,
				MultipleActiveResultSets = true,
				ApplicationName = options.ApplicationName
			}.ConnectionString;
		}

		public static async Task<DatabaseConnectionTestResult> TestAsync(
			string connectionString,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				return DatabaseConnectionTestResult.Failure(DatabaseConnectionFailureReason.MissingConnectionString);
			}

			try
			{
				var builder = new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 15 };
				await using var connection = new SqlConnection(builder.ConnectionString);
				await connection.OpenAsync(cancellationToken);
				await using var command = new SqlCommand("SELECT 1;", connection);
				await command.ExecuteScalarAsync(cancellationToken);
				return DatabaseConnectionTestResult.Success();
			}
			catch (ArgumentException ex)
			{
				return DatabaseConnectionTestResult.Failure(DatabaseConnectionFailureReason.InvalidConnectionString, ex.Message);
			}
			catch (Exception ex) when (ex is SqlException or InvalidOperationException)
			{
				return DatabaseConnectionTestResult.Failure(DatabaseConnectionFailureReason.ConnectionFailed, ex.Message);
			}
		}
	}
}
