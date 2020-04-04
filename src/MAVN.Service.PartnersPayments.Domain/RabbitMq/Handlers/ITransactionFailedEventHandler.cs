using System.Threading.Tasks;

namespace MAVN.Service.PartnersPayments.Domain.RabbitMq.Handlers
{
    public interface ITransactionFailedEventHandler
    {
        Task HandleAsync(string operationId);
    }
}
