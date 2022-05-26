using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Contracts
{
    public interface IConfigurationProvider
    {
        DataStoreType GetDataStoreType();
    }
}