using RefactorThis.DTO.Invoices;
using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Mappers.Invoices
{
    public class InvoicesMappings
    {
        /// <summary>
        /// Maps source invoice to invoiceDTO
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public GetInvoiceOutputDto MapToDto(Invoice source)
        {
            if (source is null) return null;

            return new GetInvoiceOutputDto
            {
                Reference = source.Reference,
                Amount = source.Amount,
                AmountPaid = source.AmountPaid,
                Payments = source.Payments
            };
        }
    }
}
