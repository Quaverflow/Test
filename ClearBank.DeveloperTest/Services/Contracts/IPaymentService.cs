using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Contracts
{
    public interface IPaymentService
    {
        MakePaymentResult MakePayment(MakePaymentRequest request);
    }
}
