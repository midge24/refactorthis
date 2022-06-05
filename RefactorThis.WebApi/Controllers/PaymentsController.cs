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

            InvoicesDto result;

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
                var inv = _invoiceRepository.GetInvoiceByReference(payment.Reference);
                var responseMessage = string.Empty;

                if (payment == null)
                {
                    return SendBadRequest(payment);
                }

                if (inv == null)
                {
                    responseMessage = "There is no invoice matching this payment";
                    return SendBadRequest(responseMessage);
                }

                bool isValid = inv.CheckInitialStatus(out responseMessage);
                if (!string.IsNullOrEmpty(responseMessage))
                {
                    if (isValid)
                    {
                        return Ok(responseMessage);
                    }
                    else
                    {
                        return BadRequest(responseMessage);
                    }
                }

                if (inv.InvoicePartialPaymentExists)
                {
                    responseMessage = payment.ValidatePartialPayment(inv);
                    return Ok(responseMessage);
                }
                else
                {
                    if (payment.Amount > inv.Amount)
                    {
                        responseMessage = "the payment is greater than the invoice amount";
                        return Ok(responseMessage);
                    }
                }

                return Ok(inv.TakePayment(payment));

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

        //public ActionResult<string> ProcessPaymentOld(Persistence.Entities.Payment payment)
        //{
        //    //Persistence.Entities.Invoice result2;

        //    try
        //    {
        //        if (payment != null)
        //        {
        //            var inv = _invoiceRepository.GetInvoiceByReference(payment.Reference);

        //            var responseMessage = string.Empty;

        //            if (inv == null)
        //            {
        //                //TODO: get rid of throws want process to continue if poss only stop for critical errors
        //                throw new InvalidOperationException("There is no invoice matching this payment");
        //                // responseMessage = "There is no invoice matching this payment";
        //                // return responseMessage;
        //            }
        //            else
        //            {
        //                if (inv.Amount <= 0) //==0
        //                {
        //                    if (inv.Payments == null || !inv.Payments.Any())
        //                    {
        //                        responseMessage = "no payment needed";
        //                    }
        //                    else
        //                    {
        //                        throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        //                    }
        //                }
        //                else
        //                {
        //                    if (inv.Payments != null && inv.Payments.Any())
        //                    {
        //                        var totalPaymentsMade = inv.Payments.Sum(x => x.Amount);

        //                        if (totalPaymentsMade != 0)
        //                        {
        //                            if (inv.Amount == totalPaymentsMade)
        //                            {
        //                                responseMessage = "invoice was already fully paid";
        //                            }
        //                            else if (payment.Amount > (inv.Amount - inv.AmountPaid))
        //                            {
        //                                responseMessage = "the payment is greater than the partial amount remaining";
        //                            }
        //                            else
        //                            {
        //                                if ((inv.Amount - inv.AmountPaid) == payment.Amount)
        //                                {
        //                                    responseMessage = "final partial payment received, invoice is now fully paid";
        //                                }
        //                                else
        //                                {
        //                                    responseMessage = "another partial payment received, still not fully paid";
        //                                }

        //                                inv.TakePayment(payment);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {

        //                        if (payment.Amount > inv.Amount)
        //                        {
        //                            responseMessage = "the payment is greater than the invoice amount";
        //                        }
        //                        else
        //                        {
        //                            inv.TakePayment(payment);
        //                            // inv.AmountPaid += payment.Amount; // shd be += not =
        //                            //  inv.Payments.Add(payment);

        //                            if (inv.Amount == payment.Amount)
        //                            {
        //                                responseMessage = "invoice is now fully paid";
        //                            }
        //                            else
        //                            {
        //                                responseMessage = "invoice is now partially paid";
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            // This shouldnt be here - shd be after the call in code to processpayment, only if processpayment returns ok
        //            // inv.Save();

        //            return Ok(responseMessage);
        //        }

        //        return SendBadRequest(payment);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("PaymentsController.ProcessPayment, Error: " + ex?.Message ?? "An unexpected error has occurred.", ex);
        //        return StatusCode(StatusCodes.Status500InternalServerError, "PaymentsController: Error: " + ex?.Message ?? "An unexpected error has occurred.");
        //    }
        //}


    }
}
