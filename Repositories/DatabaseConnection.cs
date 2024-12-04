using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vendor_App.Repositories

{
    class DatabaseConnection
    {
        private ITransactionRepository _transactionRepository;
        private IVendorEventRepository _vendorEventRepository;
        public string transactionsPath;
        public string eventsDbPath;

        public Vendor_App.Repositories.ITransactionRepository VendorDatabaseConnection()
        {

            transactionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "transactions.db3");
            return _transactionRepository = new SQLiteTransactionRepository(transactionsPath);
        }

        public Vendor_App.Repositories.IVendorEventRepository EventDatabaseConnection()
        {
            eventsDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
            return _vendorEventRepository = new SQLiteVendorEventRepository(eventsDbPath);
        }
    }
}
