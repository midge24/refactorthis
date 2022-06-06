using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Interfaces
{
    public interface IInvoice
    {
        string Reference { get; set; }

        decimal Amount { get; set; }

        decimal AmountPaid { get; set; }

        List<Payment> Payments { get; set; }

        decimal TotalInvoicePaymentsMade {get;}
        decimal AmountRemaining { get; }

        bool InvoicePartialPaymentExists { get; }

        bool InvoiceFullyPaid { get; }     

        string TakePayment(Payment payment);

        string CheckInitialState(Payment payment); 

        bool Save();
    }
}
