using System.Threading.Tasks;
using MAVN.Service.PartnersPayments.Domain.Models;

namespace MAVN.Service.PartnersPayments.Domain.Repositories
{
    public interface IPaymentRequestBlockchainRepository
    {
        Task UpsertAsync(string paymentRequestId, string operationId);

        Task<IPaymentRequestBlockchainData> GetByOperationIdAsync(string operationId);
    }
}
