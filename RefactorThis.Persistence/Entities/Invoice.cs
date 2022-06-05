using Newtonsoft.Json;
using RefactorThis.Persistence.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence.Entities
{
    
    public class Invoice : IInvoice
    {
        public InvoiceRepository Repository;

        [JsonConstructor]
        public Invoice()
        {

        }
       
        public Invoice(InvoiceRepository repository)
        {
            Repository = repository;
        }

        public bool Save()
        {
            return Repository.SaveInvoice(this);
        }

        /// <summary>
        /// Process payment and return appropriate responseMessage
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public string TakePayment(Payment payment)
        {
            var responseMessage = string.Empty;

            this.AmountPaid += payment.Amount;

            if (this.Payments == null)
            {
                this.Payments = new List<Payment>();
            }

            this.Payments.Add(payment);

            // Determine the responseMessage
            if (this.Amount == payment.Amount)
            {
                responseMessage = "invoice is now fully paid";
            }
            else if (payment.Amount < this.Amount)
            {
                responseMessage = "invoice is now partially paid";
            }

            return responseMessage;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public bool CheckInitialStatus(out string responseMessage)
        {
             responseMessage = string.Empty;
            var isValid = true;

            if (this.Amount == 0 && this.Payments == null)
            {
                responseMessage = "no payment needed";
            }
            else if (this.Amount <= 0)
            {
                responseMessage = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
                isValid = false;
            }
            else if (this.InvoiceFullyPaid)
            {
                responseMessage = "invoice was already fully paid";
            }

            return isValid;
        }

        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public List<Payment> Payments { get; set; }
        public decimal TotalInvoicePaymentsMade { get => this.Payments != null ? this.Payments.Sum(x => x.Amount) : 0; }

        public decimal AmountRemaining { get => this.Amount - this.AmountPaid; }

        public bool InvoicePartialPaymentExists { get => this.Payments != null && this.Payments.Any(); }

        public bool InvoiceFullyPaid { get => this.Amount == this.TotalInvoicePaymentsMade; }
        
    }
}