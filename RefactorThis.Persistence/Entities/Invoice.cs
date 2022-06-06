using Newtonsoft.Json;
using RefactorThis.Persistence.Interfaces;
using System;
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


            // Need to record partial payment status before taking payment as it will change when payment is taken
            var invoicePartialPaymentExistsBeforePayment = this.InvoicePartialPaymentExists;

            this.AmountPaid += payment.Amount;

            if (this.Payments == null)
            {
                this.Payments = new List<Payment>();
            }

            this.Payments.Add(payment);

            // Determine the responseMessage
            if (invoicePartialPaymentExistsBeforePayment)
            {
                if (this.AmountRemaining == 0) 
                {
                    responseMessage = $"Invoice {this.Reference} - final partial payment received, invoice is now fully paid";
                }
                else 
                {
                    responseMessage = $"Invoice {this.Reference} - another partial payment received, still not fully paid";
                }
            }
            else
            {
                if (this.Amount == payment.Amount)
                {
                    responseMessage = $"Invoice {this.Reference} - invoice is now fully paid";
                }
                else
                {
                    responseMessage = $"Invoice {this.Reference} - invoice is now partially paid";
                }
            }

            return responseMessage;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public string CheckInitialState(Payment payment) 
        {
            var responseMessage = string.Empty;

            if (payment.Amount < 0)
            {
                responseMessage = $"Invoice {this.Reference} - invalid payment amount ({payment.Amount})";
            }
            
            if (this.Amount == 0 && this.Payments == null)
            {
                responseMessage += $"Invoice {this.Reference} - no payment needed" + Environment.NewLine;
            }
            
            if (this.Amount <= 0)
            {
                responseMessage += $"Invoice {this.Reference} - has an invalid invoice amount ({this.Amount})" + Environment.NewLine;
            }
            
            if (this.InvoiceFullyPaid)
            {
                responseMessage += $"Invoice {this.Reference} - is already fully paid" + Environment.NewLine;
            }
            
            if (payment.Amount > this.Amount)
            {
                responseMessage += $"Invoice {this.Reference} - the payment ({payment.Amount}) is greater than the invoice amount ({this.Amount})" + Environment.NewLine;
            }
            
            if (payment.Amount > this.AmountRemaining)
            {
                responseMessage += $"Invoice {this.Reference} - the payment ({payment.Amount}) is greater than the partial amount remaining ({this.AmountRemaining})" + Environment.NewLine;
            }

            return responseMessage;
        }

        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public List<Payment> Payments { get; set; }
        public decimal TotalInvoicePaymentsMade { get => this.Payments != null ? this.Payments.Sum(x => x.Amount) : 0; }

        public decimal AmountRemaining { get => this.Amount - this.AmountPaid; }

        public bool InvoicePartialPaymentExists { get => this.Payments != null && this.Payments.Any(x => x.Amount > 0); } 

        public bool InvoiceFullyPaid { get => this.Amount == this.TotalInvoicePaymentsMade; }

    }
}