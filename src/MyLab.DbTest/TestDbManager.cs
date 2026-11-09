using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using MyLab.Db;
using Xunit.Abstractions;

namespace MyLab.DbTest
{
    internal class TestDbManager : IDbManager, IAsyncDisposable
    {
        readonly string _connectionString;
        readonly IDataProvider _dataProvider;
        readonly List<DataConnection> _connections = new List<DataConnection>();

        public ITestOutputHelper Output { get; set; }

        public TestDbManager(IDataProvider dataProvider, string connectionString)
        {
            DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public DataConnection Use(string connectionStringName = null)
        {
            var dOpts = new DataOptions()
                .UseConnectionString(_dataProvider, _connectionString)
                .UseTracing(LogTraceInfo);
            
            var dc = new DataConnection(dOpts);

            _connections.Add(dc);

            return dc;
        }

        public DataContext DoOnce(string connectionStringName = null)
        {
            var dOpts = new DataOptions()
                .UseConnectionString(_dataProvider, _connectionString)
                .UseTracing(LogTraceInfo);

            return new DataContext(dOpts);
        }

        void LogTraceInfo(TraceInfo info)
        {
            if (info.TraceInfoStep == TraceInfoStep.BeforeExecute)
            {
                Debug.WriteLine(info.SqlText);
                Output?.WriteLine(info.SqlText);
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var dc in _connections)
            {
                await dc.DisposeAsync();
            }
        }
    }
}