using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Data.Contracts
{
    public interface IDataStore
    {
        Account? GetAccount(string accountNumber);
        void UpdateAccount(Account account);
    }
}