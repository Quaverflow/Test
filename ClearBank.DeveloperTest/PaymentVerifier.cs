using System;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest
{
    public static class PaymentVerifier
    {
        public static bool IsValid(this MakePaymentRequest request, Account account)
            => request.PaymentScheme switch
            {
                PaymentScheme.Bacs => CanProcessBacs(account),
                PaymentScheme.FasterPayments => CanProcessFasterPayments(request, account),
                PaymentScheme.Chaps => CanProcessChaps(account),
                _ => throw new ArgumentOutOfRangeException(nameof(request.PaymentScheme))
            };

        private static bool CanProcessChaps(Account account)
            => account.Status == AccountStatus.Live
               && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps);

        private static bool CanProcessFasterPayments(MakePaymentRequest request, Account account)
            => account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments)
               && account.Balance >= request.Amount;

        private static bool CanProcessBacs(Account account)
            => account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);

    }
}
