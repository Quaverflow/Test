namespace ClearBank.DeveloperTest.Types
{
    public class MakePaymentResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; }

        public MakePaymentResult(string reason, bool success = false)
        {
            Reason = reason;
            Success = success;
        }
    }
}
