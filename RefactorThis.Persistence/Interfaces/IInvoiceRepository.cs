using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoiceByReference(string reference);

        bool SaveInvoice(Invoice invoice);

        void Add(Invoice invoice);
 
    }
}
