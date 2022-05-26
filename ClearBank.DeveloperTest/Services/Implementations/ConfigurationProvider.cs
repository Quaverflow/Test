using System;
using System.Configuration;
using ClearBank.DeveloperTest.Services.Contracts;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Implementations
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public DataStoreType GetDataStoreType()
        {
            var result = ConfigurationManager.AppSettings["DataStoreType"];
            if (Enum.TryParse<DataStoreType>(result, out var dataStoreType))
            {
                return dataStoreType;
            }

            throw new ArgumentException("Retrieved data store was invalid");
        }
    }
}
