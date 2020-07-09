using System.Threading.Tasks;
using LinqToDB.Data;

namespace MyLab.DbTest
{
    /// <summary>
    /// Initializes a new database
    /// </summary>
    public interface ITestDbInitializer
    {
        /// <summary>
        /// Override to implement db initialization
        /// </summary>
        Task InitializeAsync(DataConnection dataConnection);
    }
}