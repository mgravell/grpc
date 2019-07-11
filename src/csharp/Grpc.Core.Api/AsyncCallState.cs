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
using System.Threading.Tasks;

namespace Grpc.Core
{
    /// <summary>
    /// Provides an abstraction over the callback providers
    /// used by AsyncUnaryCall, AsyncDuplexStreamingCall, etc
    /// </summary>
    internal /* readonly */ struct AsyncCallState // can be made readonly in C# 7.2
    {
        readonly object compositeState; // IAsyncCall or Task<Metadata>
        readonly Func<Status> getStatusFunc; // only if not IAsyncCall-based
        readonly Func<Metadata> getTrailersFunc; // only if not IAsyncCall-based
        readonly Action disposeAction; // only if not IAsyncCall-based

        internal AsyncCallState(IAsyncCall asyncCall)
        {
            this.compositeState = asyncCall;
            this.getStatusFunc = null;
            this.getTrailersFunc = null;
            this.disposeAction = null;
        }

        internal AsyncCallState(
            Task<Metadata> responseHeadersAsync,
            Func<Status> getStatusFunc,
            Func<Metadata> getTrailersFunc,
            Action disposeAction)
        {
            this.compositeState = responseHeadersAsync;
            this.getStatusFunc = getStatusFunc;
            this.getTrailersFunc = getTrailersFunc;
            this.disposeAction = disposeAction;
        }

        internal Task<Metadata> ResponseHeadersAsync()
        {
            var asyncState = compositeState as IAsyncCall;
            return asyncState != null ? asyncState.ResponseHeadersAsync
                : (Task<Metadata>)compositeState;
        }

        internal Status GetStatus()
        {
            var asyncState = compositeState as IAsyncCall;
            return asyncState != null ? asyncState.GetStatus() : getStatusFunc();
        }

        internal Metadata GetTrailers()
        {
            var asyncState = compositeState as IAsyncCall;
            return asyncState != null ? asyncState.GetTrailers() : getTrailersFunc();
        }

        internal void Dispose()
        {
            var asyncState = compositeState as IAsyncCall;
            if (asyncState != null) asyncState.Cancel();
            else disposeAction();
        }
    }
}
