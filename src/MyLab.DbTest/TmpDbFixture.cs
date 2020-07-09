using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LinqToDB.DataProvider.SQLite;
using MyLab.Db;
using Xunit;
using Xunit.Abstractions;

namespace MyLab.DbTest
{
    /// <summary>
    /// Creates database for each tests
    /// </summary>
    public class TmpDbFixture<TDbInitializer> : TmpDbFixture, IAsyncLifetime
        where TDbInitializer : ITestDbInitializer, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TmpDbFixture"/>
        /// </summary>
        public TmpDbFixture()
            :base(new TDbInitializer())
        {
            
        }
    }

    /// <summary>
    /// Creates database for each tests
    /// </summary>
    public class TmpDbFixture : IAsyncLifetime
    {
        private readonly ITestDbInitializer _initializer;
        private readonly List<string> _dbFiles = new List<string>();
        private readonly List<TestDbManager> _dbManagers = new List<TestDbManager>();

        /// <summary>
        /// Test output
        /// </summary>
        public ITestOutputHelper Output { get; set; }

        protected TmpDbFixture(ITestDbInitializer initializer)
        {
            _initializer = initializer;
        }

        public TmpDbFixture()
            :this(null)
        {
            
        }

        /// <summary>
        /// Creates new tmp database with optional additional initializer
        /// </summary>
        public async Task<IDbManager> CreateDbAsync(ITestDbInitializer additionalInitializer = null)
        {
            var filename = $"{Guid.NewGuid():N}.db";
            var cn = $"Data Source={filename};";

            var dbManager = new TestDbManager(new SQLiteDataProvider(), cn)
            {
                Output = Output
            };

            await using var dc = dbManager.Use();

            if(_initializer != null)
                await _initializer.InitializeAsync(dc);

            if(additionalInitializer != null)
                await additionalInitializer.InitializeAsync(dc);

            _dbFiles.Add(filename);
            _dbManagers.Add(dbManager);

            return dbManager;
        }

        Task IAsyncLifetime.InitializeAsync()
        {
            return Task.CompletedTask;
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            foreach (var dbManager in _dbManagers)
            {
                await dbManager.DisposeAsync();
            }

            _dbFiles.ForEach(File.Delete);
        }
    }
}
