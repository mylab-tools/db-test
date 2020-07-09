using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using MyLab.Db;
using Xunit;
using Xunit.Abstractions;

namespace MyLab.DbTest
{
    /// <summary>
    /// The base class for fixture which provides access to remote database
    /// </summary>
    public abstract class RemoteDbFixture : IAsyncLifetime
    {
        private ITestOutputHelper _output;
        private readonly TestDbManager _dbManager;

        /// <summary>
        /// Test output
        /// </summary>
        public ITestOutputHelper Output
        {
            get => _output;
            set
            {
                _output = value;
                _dbManager.Output = value;
            }
        }

        /// <summary>
        /// Data base manager
        /// </summary>
        public IDbManager Manager => _dbManager;

        protected RemoteDbFixture(IDataProvider dataProvider, string connectionString)
        {
            _dbManager = new TestDbManager(dataProvider, connectionString);
        }

        Task IAsyncLifetime.InitializeAsync()
        {
            return Task.CompletedTask;
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            if (_dbManager != null)
                await _dbManager.DisposeAsync();
        }
    }
}