using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RefactorThis.DTO.Invoices;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Interfaces;
using RefactorThis.Services;
using RefactorThis.Services.Contracts;
using RefactorThis.WebApi.Infrastructure;

namespace RefactorThis.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors()]
    public class PaymentsController : ControllerBaseExtended
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceService _invoicesService;


        public PaymentsController(ILogger<PaymentsController> logger, IInvoiceRepository invoiceRepository, IInvoiceService invoicesService) : base(logger)
        {
            _logger = logger;
            _invoiceRepository = invoiceRepository;
            _invoicesService = invoicesService;
        }

        /// <summary>
        /// Get Invoice By Reference Using Data Transfer Object & Invoice Service
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [HttpGet("invoiceDTO/{reference}")]
        [ProducesResponseType(typeof(InvoicesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult GetInvoiceByReferenceUsingDTO(string reference)
        {

            GetInvoiceOutputDto result;

            try
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    result = _invoicesService.GetInvoiceByReference(reference);

                    return Ok(result);
                }

                return SendBadRequest(reference);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [HttpGet("invoice/{reference}")]
        [ProducesResponseType(typeof(Persistence.Entities.Invoice), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<Persistence.Entities.Invoice> GetInvoiceByReference(string reference)
        {

            Invoice result = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    result = _invoiceRepository.GetInvoiceByReference(reference);
                }

                if (result != null)
                {
                    return Ok(result);
                }

                return SendBadRequest(reference);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }
        }


        [HttpPost("invoiceDTOwithValidation")]
        [ProducesResponseType(typeof(GetInvoiceOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<GetInvoiceOutputDto> GetInvoiceByReference(GetInvoiceInputDto request)
        {

            GetInvoiceOutputDto result = null;

            try
            {
                if (this.ValidateViewModel(request))
                {
                    result = _invoicesService.GetInvoiceByReference(request.Reference);

                    return Ok(result);
                }


                //    if (!string.IsNullOrWhiteSpace(reference))
                //{
                //    result = _invoiceRepository.GetInvoiceByReference(reference);
                //}

                //if (result != null)
                //{
                //    return Ok(result);
                //}

                return SendBadRequest(request!);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }
        }

        /// <summary>
        /// Process the payment
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        [HttpPost("payment")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> ProcessPayment(Persistence.Entities.Payment payment)
        {
            try
            {
                var responseMessage = string.Empty;

                if (payment == null)
                {
                    responseMessage = "Unable to process payment - payment is null";
                    return SendBadRequest(responseMessage);
                }

                var inv = _invoiceRepository.GetInvoiceByReference(payment.InvoiceReference);

                responseMessage = inv == null ? $"Invoice {payment.InvoiceReference} - there is no invoice matching this payment" : inv.CheckInitialState(payment); 

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    return BadRequest(responseMessage);
                }

                responseMessage = inv!.TakePayment(payment);

                return Ok(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController.ProcessPayment, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }
        }

        /// <summary>
        /// Save invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        [HttpPost("invoice/save")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> Save(Persistence.Entities.Invoice invoice)
        {
            try
            {

                //TODO: Should validate the invoice

                var inv = _invoiceRepository.GetInvoiceByReference(invoice.Reference);

                if (inv != null)
                {
                    // Updating existing invoice
                    inv.Reference = invoice.Reference;
                    inv.Amount = invoice.Amount;
                    inv.AmountPaid = invoice.AmountPaid;
                    inv.Payments = invoice.Payments;
                    inv.Repository = (InvoiceRepository)_invoiceRepository;

                    if (inv.Save())
                    {
                        return Ok(inv);
                    }
                }
                else
                {
                    // new invoice
                    invoice.Repository = (InvoiceRepository)_invoiceRepository;
                    if (invoice.Save())
                    {
                        return Ok(invoice);
                    }
                }


                return SendBadRequest(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController.SaveInvoice, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }

        }

        /// <summary>
        /// Add invoice - useful for testing
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        [HttpPost("invoice/add")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> Add(Persistence.Entities.Invoice invoice)
        {
            try
            {
                _invoiceRepository.Add(invoice);

                var inv = _invoiceRepository.GetInvoiceByReference(invoice.Reference);

                if (inv != null)
                {
                    return Ok(inv);

                }

                //TODO: Validate the invoice
                return SendBadRequest(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentsController.SaveInvoice, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
            }

        }


    }
}
