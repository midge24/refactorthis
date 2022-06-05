using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Interfaces;
using System.Collections.Generic;

namespace RefactorThis.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        /// <summary>
        /// 
        /// </summary>
        public InvoiceRepository()
        {
            // For use in testing - add an invoice to repository so it can be fetched
             var invoice = new Invoice()
             {
                 Reference = "abc",
                 Amount = 10,
                 AmountPaid = 5,
                 Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 5
                        }
                 }
             };
            _invoice = invoice;
        }

        /// <summary>
        /// Get invoice form repository based on the reference passed
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public Invoice GetInvoiceByReference(string reference)
        {
            if (_invoice != null && !string.IsNullOrEmpty(reference) && _invoice.Reference == reference)
            {
                return _invoice;
            }

            return null;
        }

        /// <summary>
        /// Save Invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        public bool SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database

            return true;
        }

        /// <summary>
        /// Add invoice to repository
        /// </summary>
        /// <param name="invoice"></param>
        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }


    }
}