using RefactorThis.DTO.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Services.Contracts
{
    public interface IInvoiceService
    {
        InvoicesDto GetInvoiceByReference(string reference);
    }
}
