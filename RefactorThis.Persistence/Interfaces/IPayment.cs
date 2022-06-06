using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Interfaces
{
    public interface IPayment
    {
        decimal Amount { get; set; }
        string InvoiceReference { get; set; }

        string ValidatePartialPayment(Invoice invoice);
    }
}
