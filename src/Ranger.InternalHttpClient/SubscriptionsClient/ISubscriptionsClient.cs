using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface ISubscriptionsClient
    {
        Task<T> GenerateCheckoutExistingUrl<T>(string domain);
    }
}