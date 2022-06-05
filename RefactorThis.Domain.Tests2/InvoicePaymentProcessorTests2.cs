using NUnit.Framework;

using System;
using System.Collections.Generic;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Entities;
using Microsoft.Extensions.Logging;
using RefactorThis.WebApi.Controllers;
using RefactorThis.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RefactorThis.Services.Contracts;

[TestFixture]
public class InvoicePaymentProcessorTests2
{
    private ILogger<PaymentsController> _logger;




    [Test]
    public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
    {
        var repo = new InvoiceRepository();

        //Invoice invoice = null;
        //  var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment();
        var failureMessage = "";

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();

        _logger = factory!.CreateLogger<PaymentsController>();

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);


        try
        {
            //var result = paymentProcessor.ProcessPayment(payment);
            var preResult = controller.ProcessPayment(payment);
            failureMessage = ((ObjectResult)preResult!.Result!).Value!.ToString();
        }
        //catch (InvalidOperationException e)
        catch (Exception e)
        {
            failureMessage = e.Message;
        }

        Assert.AreEqual("There is no invoice matching this payment", failureMessage);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
    {
        var repo = new InvoiceRepository();

        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 0,
            AmountPaid = 0,
            Payments = null
        };

        repo.Add(invoice);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
   
        //   var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Reference = "abc"
        };
        var result = controller.ProcessPayment(payment);

        var finalResult = ((OkObjectResult)result!.Result!).Value;
        Assert.AreEqual("no payment needed", finalResult);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
    {
        var repo = new InvoiceRepository();

        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 10,
            AmountPaid = 10,
            Payments = new List<Payment>
            {
                new Payment
                {
                    Amount = 10
                }
            }
        };
        repo.Add(invoice);

        //  var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Reference = "abc"
        };

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        //var result = paymentProcessor.ProcessPayment(payment);

        Assert.AreEqual("invoice was already fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
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
        repo.Add(invoice);

        //  var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 6,
            Reference = "abc"
        };

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        // var result = paymentProcessor.ProcessPayment(payment);

        Assert.AreEqual("the payment is greater than the partial amount remaining", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 5,
            AmountPaid = 0,
            Payments = new List<Payment>()
        };
        repo.Add(invoice);

        //  var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 6,
            Reference = "abc"
        };

        // var result = paymentProcessor.ProcessPayment(payment);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("the payment is greater than the invoice amount", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
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
        repo.Add(invoice);

        // var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 5,
            Reference = "abc"
        };

        //  var result = paymentProcessor.ProcessPayment(payment);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 10,
            AmountPaid = 0,
            Payments = new List<Payment>()
            // Payments = new List<Payment>() { new Payment() { Amount = 10 } }
        };
        repo.Add(invoice);

        //     var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 10,
            Reference = "abc"
        };

        //    var result = paymentProcessor.ProcessPayment(payment);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("invoice is now fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
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
        repo.Add(invoice);

        // var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 1,
            Reference = "abc"
        };

        //   var result = paymentProcessor.ProcessPayment(payment);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("another partial payment received, still not fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 10,
            AmountPaid = 0,
            Payments = new List<Payment>()
        };
        repo.Add(invoice);

        //   var paymentProcessor = new InvoicePaymentProcessor(repo);

        var payment = new Payment()
        {
            Amount = 1,
            Reference = "abc"
        };

        //  var result = paymentProcessor.ProcessPayment(payment);

        var MockInvoicesService = new Mock<IInvoiceService>();

        var controller = new PaymentsController(_logger, repo, MockInvoicesService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("invoice is now partially paid", result);
    }
}
