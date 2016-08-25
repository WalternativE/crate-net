using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Net;

namespace Crate.Client
{
	public class CrateCommand : IDbCommand
	{
		private readonly CrateConnection _connection;
		private readonly CrateParameterCollection _parameters = new CrateParameterCollection();

		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }

		public IDbConnection Connection {
			get {
				return _connection;
			}
			set {
				throw new InvalidOperationException();
			}
		}

		public CrateCommand (string commandText, CrateConnection connection)
		{
			CommandText = commandText;
			_connection = connection;
		}

		#region IDbCommand implementation

		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		public IDbDataParameter CreateParameter ()
		{
			return new CrateParameter();
		}

		public int ExecuteNonQuery()
		{
			var task = ExecuteNonQueryAsync();
			task.Wait();
			return task.Result;
		}

		public async Task<int> ExecuteNonQueryAsync ()
		{
			return (await Execute()).Rowcount;
		}

		public IDataReader ExecuteReader()
		{
			var task = ExecuteReaderAsync();
			task.Wait();
			return task.Result;
		}

		protected async Task<SqlResponse> Execute(int currentRetry = 0)
		{
            var server = _connection.NextServer();
            try {
                return await SqlClient.Execute(
                        server.SqlUri(),
                        new SqlRequest(CommandText, _parameters.Select(x => x.Value).ToArray()));
            } catch (WebException) {
                _connection.MarkAsFailed(server);
                if (currentRetry > 3) {
                    throw;
                }
                return await Execute(currentRetry++);
            }
        }

		public async Task<IDataReader> ExecuteReaderAsync ()
		{
			return new CrateDataReader(await Execute());
		}

		public IDataReader ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader();
		}

		public object ExecuteScalar ()
		{
			using (var reader = ExecuteReader()) {
				reader.Read();
				return reader[0];
			}
		}

		public void Prepare ()
		{
		}


		public CommandType CommandType { get; set; }

		public IDataParameterCollection Parameters => _parameters;

	    public IDbTransaction Transaction {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public UpdateRowSource UpdatedRowSource {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
		}

		#endregion
	}
}

