using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.DTO.Invoices
{
    /// <summary>
    /// Invoices Dto
    /// Useful if we want to change the data that gets sent to the client
    /// eg may want to hide properties they shouldnt see, or remove some properties to reduce payload size etc
    /// </summary>
    public class InvoicesDto
    {
        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Payments")]
        public List<Payment> Payments { get; set; }

        [Display(Name = "Total Invoice Payments Made")]
        public decimal TotalInvoicePaymentsMade { get; }

        [Display(Name = "Amount Remaining")]
        public decimal AmountRemaining { get; }

        [Display(Name = "Invoice Partial Payment Exists")]
        public bool InvoicePartialPaymentExists { get; }

        [Display(Name = "Invoice Fully Paid")]
        public bool InvoiceFullyPaid { get; }
    }
}
