using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Data.Contracts;
using ClearBank.DeveloperTest.Services.Contracts;
using ClearBank.DeveloperTest.Services.Implementations;
using ClearBank.DeveloperTest.Types;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IAccountDataStore> _accountDataStore;
        private readonly Mock<IBackupAccountDataStore> _backupAccountDataStore;
        private readonly Mock<IConfigurationProvider> _configurationProvider;
        private PaymentService Sut
            => new PaymentService(_accountDataStore.Object, _backupAccountDataStore.Object, _configurationProvider.Object);

        public PaymentServiceTests()
        {
            _accountDataStore = new Mock<IAccountDataStore>();
            _backupAccountDataStore = new Mock<IBackupAccountDataStore>();
            _configurationProvider = new Mock<IConfigurationProvider>();
        }

        [Theory]
        [InlineData(PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs)]
        [InlineData(PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps)]
        [InlineData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments)]
        public void Should_Succeed_ForValidData(PaymentScheme paymentScheme, AllowedPaymentSchemes allowedPaymentScheme)
        {
            //Arrange
            var testAccount = GenerateAccount(allowedPaymentScheme, 1000, AccountStatus.Live);

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(paymentScheme, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.True(result.Success);
            Assert.Equal("Payment processed", result.Reason);

            var account = _accountDataStore.Object.GetAccount("");
            Assert.NotNull(account);
            Assert.Equal(0, account.Balance);
        }
        
        [Fact] 
        public void Should_Fail_ForRequestBalanceMoreThanAccountBalance()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.FasterPayments, 1000, AccountStatus.Live);

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.FasterPayments, 2000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.FasterPayments}", result.Reason);

            var account = _accountDataStore.Object.GetAccount("");
            Assert.NotNull(account);
            Assert.Equal(1000, account.Balance);
        }

        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.Chaps)]
        [InlineData(PaymentScheme.FasterPayments)]
        public void ShouldNotPass_ForNullAccount(PaymentScheme paymentScheme)
        {
            //Arrange
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>()));
            var sut = Sut;
            var request = GeneratePaymentRequest(paymentScheme, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal("Account was null", result.Reason);
        }

        [Fact]
        private void Should_UseBackup_IfConfigurationReturnsBackup()
        {
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Backup);

            var backupAccountDataStoreCalled = false;
            _backupAccountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Callback(() => backupAccountDataStoreCalled = true);

            var sut = Sut;
            sut.MakePayment(new MakePaymentRequest());

            Assert.True(backupAccountDataStoreCalled);
        }

        [Fact]
        private void Should_UseAccount_IfConfigurationReturnsOther()
        {
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var accountDataStoreCalled = false;
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Callback(() => accountDataStoreCalled = true);

            var sut = Sut;
            sut.MakePayment(new MakePaymentRequest());

            Assert.True(accountDataStoreCalled);
        }

        [Fact]
        public void ShouldNotPass_ForChaps_WhenStatusNotLive()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.Chaps, 1000, AccountStatus.Disabled);
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.Chaps, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.Chaps}", result.Reason);
        }

        [Fact]
        public void ShouldNotPass_ForChaps_WhenFlagIsNotChaps()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.Bacs, 1000, AccountStatus.Live);
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.Chaps, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.Chaps}", result.Reason);
        }

        [Fact]
        public void ShouldNotPass_ForChaps_WhenDebtorAmountIsTooLow()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.FasterPayments, 0, AccountStatus.Disabled);
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.FasterPayments, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.FasterPayments}", result.Reason);
        }

        [Fact]
        public void ShouldNotPass_ForFasterPayments_WhenFlagIsNotFasterPayments()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.Bacs, 1000, AccountStatus.Live);
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.FasterPayments, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.FasterPayments}", result.Reason);
        }

        [Fact]
        public void ShouldNotPass_ForBacs_WhenFlagIsNotBacs()
        {
            //Arrange
            var testAccount = GenerateAccount(AllowedPaymentSchemes.FasterPayments, 1000, AccountStatus.Live);
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(testAccount);
            _configurationProvider.Setup(x => x.GetDataStoreType()).Returns(DataStoreType.Account);

            var sut = Sut;
            var request = GeneratePaymentRequest(PaymentScheme.Bacs, 1000);

            //Act
            var result = sut.MakePayment(request);

            //Assert
            Assert.False(result.Success);
            Assert.Equal($"Payment failed for scheme: {PaymentScheme.Bacs}", result.Reason);
        }

        private static Account GenerateAccount(AllowedPaymentSchemes allowedPaymentScheme, int balance, AccountStatus accountStatus)
        {
            var testAccount = new Account
            {
                AccountNumber = "3456 8912 4567 0123",
                Balance = balance,
                Status = accountStatus,
                AllowedPaymentSchemes = allowedPaymentScheme
            };
            return testAccount;
        }

        private static MakePaymentRequest GeneratePaymentRequest(PaymentScheme paymentScheme, int amount)
        {
            var request = new MakePaymentRequest
            {
                CreditorAccountNumber = "0123 4567 8912 3456",
                DebtorAccountNumber = "3456 8912 4567 0123",
                Amount = amount,
                PaymentDate = DateTime.Today.AddDays(5),
                PaymentScheme = paymentScheme
            };
            return request;
        }
    }
}