using System;

namespace MAVN.Service.PartnersPayments.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ConvertToLongMs(this DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime - epoch).TotalMilliseconds;
        }
    }
}
