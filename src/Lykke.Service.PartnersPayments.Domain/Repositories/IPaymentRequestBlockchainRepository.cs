using System.Threading.Tasks;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.Domain.Repositories
{
    public interface IPaymentRequestBlockchainRepository
    {
        Task UpsertAsync(string paymentRequestId, string operationId);

        Task<IPaymentRequestBlockchainData> GetByOperationIdAsync(string operationId);
    }
}
