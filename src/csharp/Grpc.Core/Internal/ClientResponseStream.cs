#region Copyright notice and license

// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;

namespace Grpc.Core.Internal
{
    internal class ClientResponseStream<TRequest, TResponse> : IAsyncStreamReader<TResponse>
        where TRequest : class
        where TResponse : class
    {
        readonly AsyncCall<TRequest, TResponse> call;
        TResponse current;

        public ClientResponseStream(AsyncCall<TRequest, TResponse> call)
        {
            this.call = call;
        }

        public TResponse Current
        {
            get
            {
                if (current == null)
                {
                    throw new InvalidOperationException("No current element is available.");
                }
                return current;
            }
        }

        public Task<bool> MoveNext(CancellationToken token)
        {
            return MoveNextImpl(this, token);
        }

        static readonly Action<object> s_CancelToken = state => ((ClientResponseStream<TRequest, TResponse>)state).call.Cancel();
        private static async PooledTask<bool> MoveNextImpl(ClientResponseStream<TRequest, TResponse> obj, CancellationToken token)
        {
            var cancellationTokenRegistration = token.CanBeCanceled ? token.Register(s_CancelToken, obj) : default;
            using (cancellationTokenRegistration)
            {
                var result = await obj.call.ReadMessageAsync().ConfigureAwait(false);
                obj.current = result;

                if (result == null)
                {
                    await obj.call.StreamingResponseCallFinishedTask.ConfigureAwait(false);
                    return false;
                }
                return true;
            }
        }

        public void Dispose()
        {
            // TODO(jtattermusch): implement the semantics of stream disposal.
        }
    }
}
