﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvoiceServiceProvider.Factories;
using InvoiceServiceProvider.MongoDb;

namespace InvoiceServiceProvider.Services
{
    public class InvoiceService(IInvoicesRepository invoicesRepository) : InvoiceServiceProvider.InvoiceServiceContract.InvoiceServiceContractBase
    {
        private readonly IInvoicesRepository _invoicesRepository = invoicesRepository;

        public override async Task<CreateInvoiceReply> CreateInvoice(RequestCreateInvoice request, ServerCallContext context)
        {
            var invoiceEntity = InvoiceFactory.ToInvoiceEntity(request);
            if (invoiceEntity == null)
                return new CreateInvoiceReply { Succeeded = false };

            var result = await _invoicesRepository.SaveInvoiceAsync(invoiceEntity);
            return result.Succeeded
                ? new CreateInvoiceReply { Succeeded = true }
                : new CreateInvoiceReply { Succeeded = false };
        }
        public override async Task<RequestInvoiceByIdReply> GetInvoiceById(RequestInvoiceById request, ServerCallContext context)
        {
            var result = await _invoicesRepository.GetInvoiceByIdAsync(request.InvoiceId);
            if (!result.Succeeded)
                return new RequestInvoiceByIdReply { Succeeded = false };

            var invoiceModel = InvoiceFactory.ToInvoiceGrpcModel(result.Invoice!);
            return invoiceModel is not null
                ? new RequestInvoiceByIdReply { Succeeded = true, Invoice = invoiceModel }
                : new RequestInvoiceByIdReply { Succeeded = false };
        }

        public override async Task<AllInvoicesReply> GetAllInvoices(Empty request, ServerCallContext context)
        {
            AllInvoicesReply allInvoicesReply = new();

            var result = await _invoicesRepository.GetAllAsync();
            if (result.Succeeded)
            {
                allInvoicesReply.AllInvoices.AddRange(result.InvoiceList!.Select(InvoiceFactory.ToInvoiceGrpcModel));
                allInvoicesReply.Succeeded = true;
                return allInvoicesReply;
            }

            return new AllInvoicesReply { Succeeded = false };
        }

        public override async Task<UpdateInvoiceReply> UpdateInvoice(Invoice request, ServerCallContext context)
        {
            if (request is null)
                return new UpdateInvoiceReply { Succeeded = false };

            var invoiceEntity = InvoiceFactory.ToUpdateInvoiceEntity(request);
            if (invoiceEntity is null)
                return new UpdateInvoiceReply { Succeeded = false };

            var updateResult = await _invoicesRepository.UpdateAsync(invoiceEntity);
            return updateResult.Succeeded 
                ? new UpdateInvoiceReply { Succeeded = true } 
                : new UpdateInvoiceReply { Succeeded = false };
        }

        public override async Task<DeleteInvoiceReply> DeleteInvoice(DeleteInvoiceByIdRequest request, ServerCallContext context)
        {
            if (request is null)
                return new DeleteInvoiceReply { Succeeded = false };

            var deleteResult = await _invoicesRepository.DeleteAsync(request.InvoiceId);
            return deleteResult.Succeeded
                ? new DeleteInvoiceReply { Succeeded = true }
                : new DeleteInvoiceReply { Succeeded = false };
        }
    }
}
