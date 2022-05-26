using ClearBank.DeveloperTest.Data.Contracts;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Data.Implementations
{
    public class BackupAccountDataStore : IBackupAccountDataStore
    {
        public Account GetAccount(string accountNumber)
        {
            // Access backup data base to retrieve account, code removed for brevity 
            return new Account();
        }

        public void UpdateAccount(Account account)
        {
            // Update account in backup database, code removed for brevity
        }
    }
}
