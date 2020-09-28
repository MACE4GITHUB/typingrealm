﻿using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public interface IServiceClient
    {
        ValueTask<T> GetAsync<T>(string serviceName, string endpoint, CancellationToken cancellationToken);
    }
}
