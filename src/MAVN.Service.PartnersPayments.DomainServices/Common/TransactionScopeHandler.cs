using System;
using System.Threading.Tasks;
using System.Transactions;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PartnersPayments.Domain.Common;

namespace MAVN.Service.PartnersPayments.DomainServices.Common
{
    public class TransactionScopeHandler : ITransactionScopeHandler
    {
        private readonly ILog _log;

        public TransactionScopeHandler(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }
        public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await func();

                    scope.Complete();

                    return result;
                }
            }
            catch (TransactionAbortedException e)
            {
                _log.Error(e, "Error occured while commiting transaction");

                throw;
            }
        }

        public async Task WithTransactionAsync(Func<Task> action)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await action();

                    scope.Complete();
                }
            }
            catch (TransactionAbortedException e)
            {
                _log.Error(e, "Error occured while commiting transaction");

                throw;
            }
        }
    }
}
