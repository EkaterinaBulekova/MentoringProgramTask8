using DALLayer.DALInterfaces;

namespace DALLayer.DALConcrete
{
    public class DatabaseContextFactory : IDataContextFactory
    {
        private IDataContext _dataContext;

        public IDataContext Context()
        {
            return _dataContext ?? (_dataContext = new DataContext());
        }

        public void Dispose()
        {
            _dataContext?.Dispose();
        }
    }
}
