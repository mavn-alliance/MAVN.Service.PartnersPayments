using System.Threading.Tasks;

namespace Lykke.Service.PartnersPayments.Domain.RabbitMq.Handlers
{
    public interface ITransactionFailedEventHandler
    {
        Task HandleAsync(string operationId);
    }
}
