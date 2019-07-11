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

using System.Threading.Tasks;

namespace Grpc.Core
{
    /// <summary>
    /// Represents an abstract async RPC call
    /// </summary>
    public interface IAsyncCall
    {
        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        Task<Metadata> ResponseHeadersAsync { get; }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// </summary>
        Status GetStatus();

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// </summary>
        Metadata GetTrailers();

        /// <summary>
        /// Provides means to cleanup after the call.
        /// If the call has already finished normally (request stream has been completed and call result has been received), doesn't do anything.
        /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
        /// As a result, all resources being used by the call should be released eventually.
        /// </summary>
        void Cancel();
    }
}
