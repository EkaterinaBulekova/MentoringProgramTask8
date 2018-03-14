using System.Configuration;
using System.Data;
using System.Data.Common;
using DALLayer.DALInterfaces;

namespace DALLayer.DALConcrete
{
    public class DataContext : IDataContext
    {
        private readonly DbProviderFactory _providerFactory;
        private readonly string _connectionString;
        private DbConnection _connection;

        public DataContext()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["NorthwindConection"].ConnectionString;
            _providerFactory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings["NorthwindConection"].ProviderName);
        }

        public DbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _providerFactory.CreateConnection();
                    if (_connection != null)
                    {
                        _connection.ConnectionString = _connectionString;
                    }
                }
                if (_connection != null && _connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}
