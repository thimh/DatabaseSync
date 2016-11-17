using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;

namespace MicrosoftSyncFramework_Server
{
    class Program
    {
        private static string sServerConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\ParkInspect_Server.mdf;Integrated Security=True";//@"Data Source=Server\MSSQL2008;Initial Catalog=Company;Persist Security Info=False;User ID=sa;Password=password;Connect Timeout=60";

        private static string sClientConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\ParkInspect_Client.mdf;Integrated Security=True";//@"Data Source=Client\MSSQL2008;Initial Catalog=Company;Persist Security Info=False;User ID=sa;Password=password;Connect Timeout=60";

        static string sScope = "UsersScope1";

        static void Main(string[] args)
        {
            ProvisionServer();
            ProvisionClient();
            Sync();
        }

        public static void ProvisionServer()

        {

            SqlConnection serverConn = new SqlConnection(sServerConnection);



            DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription(sScope);



            DbSyncTableDescription tableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable("Users", serverConn);

            scopeDesc.Tables.Add(tableDesc);



            SqlSyncScopeProvisioning serverProvision = new SqlSyncScopeProvisioning(serverConn, scopeDesc);

            serverProvision.SetCreateTableDefault(DbSyncCreationOption.Skip);



            serverProvision.Apply();

        }

        private static void ProvisionClient()
        {
            SqlConnection serverConn = new SqlConnection(sServerConnection);

            SqlConnection clientConn = new SqlConnection(sClientConnection);



            DbSyncScopeDescription scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(sScope, serverConn);

            SqlSyncScopeProvisioning clientProvision = new SqlSyncScopeProvisioning(clientConn, scopeDesc);



            clientProvision.Apply();
        }

        private static void Sync()
        {
            SqlConnection serverConn = new SqlConnection(sServerConnection);

            SqlConnection clientConn = new SqlConnection(sClientConnection);



            SyncOrchestrator syncOrchestrator = new SyncOrchestrator();



            syncOrchestrator.LocalProvider = new SqlSyncProvider(sScope, clientConn);

            syncOrchestrator.RemoteProvider = new SqlSyncProvider(sScope, serverConn);



            syncOrchestrator.Direction = SyncDirectionOrder.DownloadAndUpload;



            ((SqlSyncProvider)syncOrchestrator.LocalProvider).ApplyChangeFailed += new EventHandler<DbApplyChangeFailedEventArgs>(Program_ApplyChangeFailed);



            SyncOperationStatistics syncStats = syncOrchestrator.Synchronize();



            Console.WriteLine("Start Time: " + syncStats.SyncStartTime);

            Console.WriteLine("Total Changes Uploaded: " + syncStats.UploadChangesTotal);

            Console.WriteLine("Total Changes Downloaded: " + syncStats.DownloadChangesTotal);

            Console.WriteLine("Complete Time: " + syncStats.SyncEndTime);

            Console.WriteLine(String.Empty);

            Console.ReadLine();
        }

        static void Program_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)

        {

            Console.WriteLine(e.Conflict.Type);

            Console.WriteLine(e.Error);

        }
    }
}
