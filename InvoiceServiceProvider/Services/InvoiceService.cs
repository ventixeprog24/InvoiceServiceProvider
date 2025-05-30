﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvoiceServiceProvider.Factories;
using InvoiceServiceProvider.MongoDb;
using EmailServiceClient = EmailServiceProvider.EmailServicer.EmailServicerClient;

namespace InvoiceServiceProvider.Services
{
    public class InvoiceService(IInvoicesRepository invoicesRepository, IPdfService pdfService, EmailServiceClient emailServiceClient,
        EmailFactory emailFactory ) : InvoiceServiceContract.InvoiceServiceContractBase
    {
        private readonly IInvoicesRepository _invoicesRepository = invoicesRepository;
        private readonly IPdfService _pdfService = pdfService;
        private readonly EmailServiceClient _emailServiceClient = emailServiceClient;
        private readonly EmailFactory _emailFactory = emailFactory;

        public override async Task<CreateInvoiceReply> CreateInvoice(RequestCreateInvoice request, ServerCallContext context)
        {
            if (request is null)
                return new CreateInvoiceReply { Succeeded = false };
            
            var invoiceEntity = InvoiceFactory.ToInvoiceEntity(request);
            if (invoiceEntity is null)
                return new CreateInvoiceReply { Succeeded = false };

            var result = await _invoicesRepository.SaveInvoiceAsync(invoiceEntity);
            if (!result.Succeeded)
                return new CreateInvoiceReply { Succeeded = false };
            
            var pdfResult = await _pdfService.GeneratePdfAsync(invoiceEntity);
            if (!pdfResult.Succeeded || string.IsNullOrWhiteSpace(pdfResult.Uri)) 
                return  new CreateInvoiceReply { Succeeded = false };

            try
            {
                var emailRequest = _emailFactory.CreateEmailRequest(invoiceEntity, pdfResult.Uri);
                var emailResult =  await _emailServiceClient.SendEmailAsync(emailRequest);
                return emailResult.Succeeded 
                    ? new CreateInvoiceReply { Succeeded = true }
                    : new CreateInvoiceReply { Succeeded = false };
            }
            catch (Exception ex)
            {
                return new CreateInvoiceReply { Succeeded = false };
            }
        }
        
        public async Task<CreateInvoiceReply> CreateInvoiceFromServiceBus(RequestCreateInvoice request)
        {
            if (request is null)
                return new CreateInvoiceReply { Succeeded = false };
            
            var invoiceEntity = InvoiceFactory.ToInvoiceEntity(request);
            if (invoiceEntity is null)
                return new CreateInvoiceReply { Succeeded = false };

            var result = await _invoicesRepository.SaveInvoiceAsync(invoiceEntity);
            if (!result.Succeeded)
                return new CreateInvoiceReply { Succeeded = false };
            
            var pdfResult = await _pdfService.GeneratePdfAsync(invoiceEntity);
            if (!pdfResult.Succeeded || string.IsNullOrWhiteSpace(pdfResult.Uri))
            {
                await _invoicesRepository.HardDeleteFromDbAsync(invoiceEntity.Id);
                return  new CreateInvoiceReply { Succeeded = false };
            }

            try
            {
                var emailRequest = _emailFactory.CreateEmailRequest(invoiceEntity, pdfResult.Uri);
                var emailResult =  await _emailServiceClient.SendEmailAsync(emailRequest);
                if (emailResult.Succeeded)
                    return new CreateInvoiceReply { Succeeded = true };
                
                await _invoicesRepository.HardDeleteFromDbAsync(invoiceEntity.Id);
                return new CreateInvoiceReply { Succeeded = false };
            }
            catch (Exception ex)
            {
                return new CreateInvoiceReply { Succeeded = false };
            }
        }
        
        public override async Task<RequestInvoiceByIdReply> GetInvoiceByInvoiceId(RequestInvoiceById request, ServerCallContext context)
        {
            if (request is null)
                return new RequestInvoiceByIdReply { Succeeded = false };
            
            var result = await _invoicesRepository.GetInvoiceByInvoiceIdAsync(request.Id);
            if (!result.Succeeded)
                return new RequestInvoiceByIdReply { Succeeded = false };

            var invoiceModel = InvoiceFactory.ToInvoiceGrpcModel(result.Invoice!);
            return invoiceModel is not null
                ? new RequestInvoiceByIdReply { Succeeded = true, Invoice = invoiceModel }
                : new RequestInvoiceByIdReply { Succeeded = false };
        }

        public override async Task<RequestInvoiceByIdReply> GetInvoiceByBookingId(RequestInvoiceById request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Id))
                return new RequestInvoiceByIdReply { Succeeded = false };
            
            var result = await _invoicesRepository.GetInvoiceByBookingIdAsync(request.Id);
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

        public override async Task<UpdateInvoiceReply> UpdateInvoice(UpdatePaymentStatusRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.InvoiceId))
                return new UpdateInvoiceReply { Succeeded = false };

            var updateResult = await _invoicesRepository.UpdateAsync(request);
            return updateResult.Succeeded
                ? new UpdateInvoiceReply { Succeeded = true }
                : new UpdateInvoiceReply { Succeeded = false };
        }

        public override async Task<DeleteInvoiceReply> DeleteInvoice(DeleteInvoiceByIdRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.InvoiceId))
                return new DeleteInvoiceReply { Succeeded = false };

            var deleteResult = await _invoicesRepository.DeleteAsync(request.InvoiceId);
            return deleteResult.Succeeded
                ? new DeleteInvoiceReply { Succeeded = true }
                : new DeleteInvoiceReply { Succeeded = false };
        }
    }
}
