using RefactorThis.Persistence.Interfaces;

namespace RefactorThis.Persistence.Entities
{
    public class Payment: IPayment
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }

        /// <summary>
        /// Validate a partial payment
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns>responseMessage</returns>
        public string ValidatePartialPayment(Invoice invoice)
        {
            var responseMessage = string.Empty;

            if (invoice.InvoiceFullyPaid)
            {
                responseMessage = "invoice was already fully paid";
            }

            if (this.Amount > invoice.AmountRemaining)
            {
                responseMessage = "the payment is greater than the partial amount remaining";
            }
            else if (this.Amount == invoice.AmountRemaining)
            {
                responseMessage = "final partial payment received, invoice is now fully paid";
            }
            else
            {
                responseMessage = "another partial payment received, still not fully paid";
            }

            return responseMessage;
        }
    }
}