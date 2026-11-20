using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;

namespace Vylersant_Facturacion.Application.Invoices
{
    public sealed class CancelInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public CancelInvoiceService(IInvoiceRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task ExecuteAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
            {
                throw new ArgumentException("El identificador de la factura es obligatorio", nameof(invoiceId));
            }

            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("El identificador del negocio es obligatorio", nameof(businessId));
            }

            var invoice = await _repository.GetByIdAsync(invoiceId, businessId, cancellationToken);

            if (invoice is null)
            {
                throw new InvoiceNotFoundException(invoiceId);
            }

            if (invoice.Status == Vylersant_Facturacion.Domain.Entities.Invoices.InvoiceStatus.Cancelled)
            {
                return;
            }

            invoice.Cancel(_timeProvider.GetUtcNow().UtcDateTime);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

