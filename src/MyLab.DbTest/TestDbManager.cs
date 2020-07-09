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
    class TestDbManager : IDbManager, IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly IDataProvider _dataProvider;
        private readonly List<DataConnection> _connections = new List<DataConnection>();

        public ITestOutputHelper Output { get; set; }

        public TestDbManager(IDataProvider dataProvider, string connectionString)
        {
            DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public DataConnection Use(string connectionStringName = null)
        {
            var dc = new DataConnection(_dataProvider, _connectionString);

            dc.OnTraceConnection += LogTraceInfo;
            _connections.Add(dc);

            return dc;
        }

        public DataContext DoOnce(string connectionStringName = null)
        {
            var dc =  new DataContext(_dataProvider, _connectionString);

            dc.OnTraceConnection += LogTraceInfo;

            return dc;
        }

        private void LogTraceInfo(TraceInfo info)
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
                dc.OnTraceConnection = null;
                await dc.DisposeAsync(CancellationToken.None);
            }
        }
    }
}