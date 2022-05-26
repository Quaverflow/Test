using ClearBank.DeveloperTest.Data.Contracts;
using ClearBank.DeveloperTest.Services.Contracts;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;
        private readonly IBackupAccountDataStore _backupAccountDataStore;
        private readonly IConfigurationProvider _configurationProvider;

        public PaymentService(IAccountDataStore accountDataStore, IBackupAccountDataStore backupAccountDataStore, IConfigurationProvider configurationProvider)
        {
            _accountDataStore = accountDataStore;
            _backupAccountDataStore = backupAccountDataStore;
            _configurationProvider = configurationProvider;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var dataStoreType = _configurationProvider.GetDataStoreType();
            var selectedDataStore = SelectDataStore(dataStoreType);

            var account = selectedDataStore.GetAccount(request.DebtorAccountNumber);

            if (account == null)
            {
                return new MakePaymentResult("Account was null");
            }

            if(request.IsValid(account))
            {
                account.Balance -= request.Amount;
                selectedDataStore.UpdateAccount(account);
                return new MakePaymentResult("Payment processed", true);
            }

            return new MakePaymentResult($"Payment failed for scheme: {request.PaymentScheme}");
        }

        private IDataStore SelectDataStore(DataStoreType dataStoreType)
            => dataStoreType == DataStoreType.Backup ? _backupAccountDataStore : _accountDataStore;

    }
}
