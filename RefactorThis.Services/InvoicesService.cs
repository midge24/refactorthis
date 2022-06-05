using Microsoft.Extensions.Logging;
using RefactorThis.DTO.Invoices;
using RefactorThis.Mappers.Invoices;
using RefactorThis.Persistence.Interfaces;
using RefactorThis.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Services
{
    public class InvoicesService : IInvoiceService
    {
        ILogger Logger { get; }
        IInvoiceRepository InvoiceRepository { get; }
        InvoicesMappings InvoicesMappings { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="repository"></param>
        /// <param name="invoiceMappings"></param>
        public InvoicesService(ILogger<InvoicesService> logger, IInvoiceRepository repository, InvoicesMappings invoiceMappings)
        {
            Logger = logger;
            InvoiceRepository = repository;
            InvoicesMappings = invoiceMappings;
        } 

        //TODO: add async methods

        /// <summary>
        /// Get Invoice by Reference
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public virtual InvoicesDto GetInvoiceByReference(string reference)
        {
            InvoicesDto result;

            try
            {
                var invoice = InvoiceRepository.GetInvoiceByReference(reference) ?? throw new Exception("Unknown Invoice Reference");

                result = InvoicesMappings.MapToDto(invoice);
            }
            catch (Exception ex)
            {
                Logger.LogError($"InvoicesService.GetInvoiceByReference: {ex?.Message}", ex);
                throw;
            }

            return result;
        }
    }
}
