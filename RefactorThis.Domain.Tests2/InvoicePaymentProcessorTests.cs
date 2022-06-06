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
public class InvoicePaymentProcessorTests
{
    private Mock<ILogger<PaymentsController>> MockLogger;
    private Mock<IInvoiceService> MockInvoiceService;

    public InvoicePaymentProcessorTests()
    {
        MockInvoiceService = new Mock<IInvoiceService>();
        MockLogger = new Mock<ILogger<PaymentsController>>();
    }


    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_PaymentIsNull()
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

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);

        var preResult = controller.ProcessPayment(null);

        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        Assert.AreEqual("Unable to process payment - payment is null", result);
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

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);

        var payment = new Payment()
        {
            InvoiceReference = "abc"
        };

        var preResult = controller.ProcessPayment(payment);

        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - no payment needed", result!.ToString());

    }

    #region --- Invoice tests ---
    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_NegativePaymentAmount()
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

        var payment = new Payment()
        {
            Amount = -6,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        Assert.AreEqual($"Invoice {invoice.Reference} - invalid payment amount ({payment.Amount})", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAmountZero()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = 0,
            AmountPaid = 0,
            Payments = new List<Payment>()
        };
        repo.Add(invoice);

        var payment = new Payment()
        {
            Amount = 6,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - has an invalid invoice amount ({invoice.Amount})", result!.ToString());
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAmountNegative()
    {
        var repo = new InvoiceRepository();
        var invoice = new Invoice(repo)
        {
            Reference = "abc",
            Amount = -10,
            AmountPaid = 0,
            Payments = new List<Payment>()
        };
        repo.Add(invoice);

        var payment = new Payment()
        {
            Amount = 6,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - has an invalid invoice amount ({invoice.Amount})", result!.ToString());
    }

    [Test]
    public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
    {
        var repo = new InvoiceRepository();

        var payment = new Payment();
        var failureMessage = "";

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();

        var _logger = factory!.CreateLogger<PaymentsController>();

        var controller = new PaymentsController(_logger, repo, MockInvoiceService.Object);

        try
        {
            var preResult = controller.ProcessPayment(payment);
            failureMessage = ((BadRequestObjectResult)preResult!.Result!).Value!.ToString();
        }
        catch (Exception e)
        {
            failureMessage = e.Message;
        }

        Assert.AreEqual($"Invoice {payment.InvoiceReference} - there is no invoice matching this payment", failureMessage);
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

        var payment = new Payment()
        {
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - is already fully paid", result!.ToString());
    }

    #endregion

    #region --- Partial Payment Tests ---
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
        repo.Add(invoice); ;

        var payment = new Payment()
        {
            Amount = 6,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - the payment ({payment.Amount}) is greater than the partial amount remaining ({invoice.AmountRemaining})", result!.ToString());
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

        var payment = new Payment()
        {
            Amount = 5,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual($"Invoice {invoice.Reference} - final partial payment received, invoice is now fully paid", result);
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

        var payment = new Payment()
        {
            Amount = 1,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual($"Invoice {invoice.Reference} - another partial payment received, still not fully paid", result);
    }

    #endregion

    #region --- No Partial Payment Tests ---
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

        var payment = new Payment()
        {
            Amount = 6,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((BadRequestObjectResult)preResult!.Result!).Value;

        StringAssert.Contains($"Invoice {invoice.Reference} - the payment ({payment.Amount}) is greater than the invoice amount ({invoice.Amount})", result!.ToString());
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

        };
        repo.Add(invoice);

        var payment = new Payment()
        {
            Amount = 10,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual($"Invoice {invoice.Reference} - invoice is now fully paid", result);
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

        var payment = new Payment()
        {
            Amount = 1,
            InvoiceReference = "abc"
        };

        var controller = new PaymentsController(MockLogger.Object, repo, MockInvoiceService.Object);
        var preResult = controller.ProcessPayment(payment);
        var result = ((OkObjectResult)preResult!.Result!).Value;

        Assert.AreEqual($"Invoice {invoice.Reference} - invoice is now partially paid", result);
    }

    #endregion
}
