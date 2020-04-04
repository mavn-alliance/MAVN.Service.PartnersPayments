using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PartnersPayments.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Job.PartnersPayments.Services
{
    public class ExpiredRequestsManager : IStartable, IStopable
    {
        private readonly IPaymentsService _paymentsService;
        private readonly TimeSpan _paymentsExpirationPeriod;
        private readonly TimerTrigger _timerTrigger;
        private readonly ILog _log;

        public ExpiredRequestsManager(
            IPaymentsService paymentsService,
            TimeSpan idlePeriod,
            TimeSpan paymentsExpirationPeriod,
            ILogFactory logFactory)
        {
            _paymentsService = paymentsService;
            _paymentsExpirationPeriod = paymentsExpirationPeriod;
            _log = logFactory.CreateLog(this);
            _timerTrigger = new TimerTrigger(nameof(ExpiredRequestsManager), idlePeriod, logFactory);
            _timerTrigger.Triggered += Execute;
        }

        public void Start()
        {
            _timerTrigger.Start();
        }

        public void Stop()
        {
            _timerTrigger?.Stop();
        }

        public void Dispose()
        {
            _timerTrigger?.Stop();
            _timerTrigger?.Dispose();
        }

        private async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            var paymentsTask = _paymentsService.MarkPaymentsAsExpiredAsync(_paymentsExpirationPeriod);
            var requestsTask = _paymentsService.MarkRequestsAsExpiredAsync();

            _log.Info("Starting checking and marking partners payments as expired");
            await Task.WhenAll(paymentsTask, requestsTask);
            _log.Info("Finished checking and marking partners payments as expired");
        }
    }
}
