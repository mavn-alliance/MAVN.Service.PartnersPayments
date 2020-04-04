using System;
using System.Threading.Tasks;

namespace MAVN.Service.PartnersPayments.Domain.RabbitMq.Handlers
{
    public interface IUndecodedEventHandler
    {
        Task HandleAsync(string[] topics, string data, string contractAddress, DateTime timestamp);
    }
}
