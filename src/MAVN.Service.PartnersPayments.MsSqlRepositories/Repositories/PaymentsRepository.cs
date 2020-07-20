using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly PostgreSQLContextFactory<PartnersPaymentsContext> _contextFactory;

        public PaymentsRepository(PostgreSQLContextFactory<PartnersPaymentsContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(IPaymentRequest paymentRequest)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = PartnerPaymentEntity.Create(paymentRequest);

                context.PartnersPayments.Add(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task SetStatusAsync(string paymentRequestId, PaymentRequestStatus status)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = new PartnerPaymentEntity { PaymentRequestId = paymentRequestId };

                context.PartnersPayments.Attach(entity);

                entity.Status = status;
                entity.LastUpdatedTimestamp = DateTime.UtcNow;

                try
                {
                    await context.SaveChangesAsync();

                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException("Entity was not found during status update");
                }
            }
        }

        public async Task UpdatePaymentAsync(PaymentModel paymentModel)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = PartnerPaymentEntity.Create(paymentModel);

                entity.LastUpdatedTimestamp = DateTime.UtcNow;

                context.PartnersPayments.Update(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task<PaymentModel> GetByPaymentRequestIdAsync(string paymentRequestId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.PartnersPayments.FindAsync(paymentRequestId);

                if (entity == null)
                    return null;

                return new PaymentModel
                {
                    CustomerId = entity.CustomerId,
                    FiatAmount = entity.FiatAmount,
                    PaymentRequestId = entity.PaymentRequestId,
                    LocationId = entity.LocationId,
                    TokensReserveTimestamp = entity.TokensReserveTimestamp,
                    TokensBurnTimestamp = entity.TokensBurnTimestamp,
                    PartnerMessageId = entity.PartnerMessageId,
                    TokensAmount = entity.TokensAmount,
                    PartnerId = entity.PartnerId,
                    Currency = entity.Currency,
                    TokensSendingAmount = entity.TokensAmountPaidByCustomer,
                    Status = entity.Status,
                    TotalBillAmount = entity.TotalBillAmount,
                    Timestamp = entity.Timestamp,
                    PosId = entity.PosId,
                    TokensToFiatConversionRate = entity.TokensToFiatConversionRate,
                    LastUpdatedTimestamp = entity.LastUpdatedTimestamp,
                    FiatSendingAmount = entity.FiatAmountPaidByCustomer,
                    CustomerActionExpirationTimestamp = entity.CustomerActionExpirationTimestamp,
                };
            }
        }

        public async Task<(IEnumerable<PaymentModel>, int)> GetPendingPaymentRequestsForCustomerAsync(string customerId, int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var whereResult = context.PartnersPayments.Where(x =>
                        x.CustomerId == customerId &&
                       (x.Status == PaymentRequestStatus.Created ||
                        x.Status == PaymentRequestStatus.TokensTransferStarted ||
                        x.Status == PaymentRequestStatus.TokensTransferSucceeded ||
                        x.Status == PaymentRequestStatus.TokensRefundStarted ||
                        x.Status == PaymentRequestStatus.ExpirationTokensRefundStarted ||
                        x.Status == PaymentRequestStatus.TokensBurnStarted));

                var totalCount = await whereResult.CountAsync();

                var result = await whereResult
                    .OrderByDescending(x => x.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .Select(GetInfoForStatusRequestsSelectExpression())
                    .ToListAsync();

                return (result, totalCount);
            }
        }

        public async Task<(IEnumerable<PaymentModel>, int)> GetSucceededPaymentRequestsForCustomerAsync(string customerId, int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var whereResult = context.PartnersPayments.Where(x =>
                    x.CustomerId == customerId &&
                    x.Status == PaymentRequestStatus.TokensBurnSucceeded);

                var totalCount = await whereResult.CountAsync();

                var result = await whereResult
                    .OrderByDescending(x => x.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .Select(GetInfoForStatusRequestsSelectExpression())
                    .ToListAsync();

                return (result, totalCount);
            }
        }

        public async Task<(IEnumerable<PaymentModel>, int)> GetFailedPaymentRequestsForCustomerAsync(string customerId, int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var whereResult = context.PartnersPayments.Where(x =>
                    x.CustomerId == customerId &&
                   (x.Status == PaymentRequestStatus.RejectedByCustomer ||
                    x.Status == PaymentRequestStatus.TokensTransferFailed ||
                    x.Status == PaymentRequestStatus.TokensBurnFailed ||
                    x.Status == PaymentRequestStatus.TokensRefundFailed ||
                    x.Status == PaymentRequestStatus.TokensRefundSucceeded ||
                    x.Status == PaymentRequestStatus.RequestExpired ||
                    x.Status == PaymentRequestStatus.ExpirationTokensRefundSucceeded ||
                    x.Status == PaymentRequestStatus.ExpirationTokensRefundFailed ||
                    x.Status == PaymentRequestStatus.CancelledByPartner));

                var totalCount = await whereResult.CountAsync();

                var result = await whereResult
                    .OrderByDescending(x => x.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .Select(GetInfoForStatusRequestsSelectExpression())
                    .ToListAsync();

                return (result, totalCount);
            }
        }

        public async Task<string[]> GetExpiredRequestsAsync()
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.PartnersPayments
                    .Where(p => p.Status == PaymentRequestStatus.Created &&
                                p.CustomerActionExpirationTimestamp < DateTime.UtcNow)
                    .Select(p => p.PaymentRequestId)
                    .ToArrayAsync();

                return result;
            }
        }

        public async Task<string[]> GetExpiredPaymentsAsync(DateTime expirationDate)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.PartnersPayments
                    .Where(p => p.Status == PaymentRequestStatus.TokensTransferSucceeded &&
                                p.LastUpdatedTimestamp < expirationDate)
                    .Select(p => p.PaymentRequestId)
                    .ToArrayAsync();

                return result;
            }
        }

        private static Expression<Func<PartnerPaymentEntity, PaymentModel>> GetInfoForStatusRequestsSelectExpression()
        {
            return x => new PaymentModel
            {
                CustomerId = x.CustomerId,
                FiatAmount = x.FiatAmount,
                PaymentRequestId = x.PaymentRequestId,
                LocationId = x.LocationId,
                PartnerMessageId = x.PartnerMessageId,
                TokensAmount = x.TokensAmount,
                PartnerId = x.PartnerId,
                Currency = x.Currency,
                TokensSendingAmount = x.TokensAmountPaidByCustomer,
                Status = x.Status,
                Timestamp = x.Timestamp,
                LastUpdatedTimestamp = x.LastUpdatedTimestamp,
                FiatSendingAmount = x.FiatAmountPaidByCustomer,
                TotalBillAmount = x.TotalBillAmount,
                CustomerActionExpirationTimestamp = x.CustomerActionExpirationTimestamp,
            };
        }

    }
}
