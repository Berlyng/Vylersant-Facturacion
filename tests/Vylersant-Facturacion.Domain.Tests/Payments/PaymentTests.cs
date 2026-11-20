using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Domain.Tests.Payments;

public sealed class PaymentTests
{
    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Card)]
    [InlineData(PaymentMethod.Transfer)]
    public void Constructor_ShouldCreateCompletedPayment(PaymentMethod method)
    {
        var businessId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var paidAtUtc = DateTime.UtcNow;

        var payment = new Payment(businessId, invoiceId, 12.345m, paidAtUtc, method, "  REF-1  ");

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(businessId, payment.BusinessId);
        Assert.Equal(invoiceId, payment.InvoiceId);
        Assert.Equal(12.35m, payment.Amount);
        Assert.Equal(paidAtUtc, payment.PaidAtUtc);
        Assert.Equal(method, payment.Method);
        Assert.Equal("REF-1", payment.Reference);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Null(payment.VoidedAtUtc);
    }

    [Fact]
    public void Constructor_ShouldNormalizeBlankReferenceToNull()
    {
        var payment = CreatePayment(reference: "  ");

        Assert.Null(payment.Reference);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectNonPositiveCurrencyAmount(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), amount, DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void Constructor_ShouldRejectAmountThatRoundsToZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), 0.004m, DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyBusinessOrInvoiceId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Payment(Guid.Empty, Guid.NewGuid(), 1m, DateTime.UtcNow, PaymentMethod.Cash));
        Assert.Throws<ArgumentException>(() =>
            new Payment(Guid.NewGuid(), Guid.Empty, 1m, DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void MultiplePayments_CanReferToTheSameInvoice()
    {
        var businessId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        var cashPayment = new Payment(businessId, invoiceId, 5m, DateTime.UtcNow, PaymentMethod.Cash);
        var transferPayment = new Payment(businessId, invoiceId, 5m, DateTime.UtcNow, PaymentMethod.Transfer);

        Assert.Equal(invoiceId, cashPayment.InvoiceId);
        Assert.Equal(invoiceId, transferPayment.InvoiceId);
        Assert.NotEqual(cashPayment.Id, transferPayment.Id);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidMethod()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), 1m, DateTime.UtcNow, (PaymentMethod)99));
    }

    [Fact]
    public void Void_ShouldPreservePaymentAndBeIdempotent()
    {
        var payment = CreatePayment();
        var voidedAtUtc = DateTime.UtcNow;

        payment.Void(voidedAtUtc);
        payment.Void(voidedAtUtc.AddMinutes(1));

        Assert.Equal(PaymentStatus.Voided, payment.Status);
        Assert.Equal(voidedAtUtc, payment.VoidedAtUtc);
        Assert.Equal(10m, payment.Amount);
        Assert.Equal("Pago", payment.Reference);
    }

    private static Payment CreatePayment(string? reference = "Pago") =>
        new(Guid.NewGuid(), Guid.NewGuid(), 10m, DateTime.UtcNow, PaymentMethod.Cash, reference);
}

