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
using System.Threading.Tasks;
using PooledAwait;

namespace Grpc.Core.Utils
{
    /// <summary>
    /// Extension methods that simplify work with gRPC streaming calls.
    /// </summary>
    public static class AsyncStreamExtensions
    {
        /// <summary>
        /// Reads the entire stream and executes an async action for each element.
        /// </summary>
        public static Task ForEachAsync<T>(this IAsyncStreamReader<T> streamReader, Func<T, Task> asyncAction)
            where T : class
        {
            return ForEachAsyncImpl<T>(streamReader, asyncAction);
        }
        private static async PooledTask ForEachAsyncImpl<T>(IAsyncStreamReader<T> streamReader, Func<T, Task> asyncAction)
        {
            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                await asyncAction(streamReader.Current).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reads the entire stream and creates a list containing all the elements read.
        /// </summary>
        public static Task<List<T>> ToListAsync<T>(this IAsyncStreamReader<T> streamReader)
            where T : class
        {
            return ToListAsyncImpl<T>(streamReader);
        }
        private static async PooledTask<List<T>> ToListAsyncImpl<T>(IAsyncStreamReader<T> streamReader)
        {
            var result = new List<T>();
            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                result.Add(streamReader.Current);
            }
            return result;
        }

        /// <summary>
        /// Writes all elements from given enumerable to the stream.
        /// Completes the stream afterwards unless close = false.
        /// </summary>
        public static Task WriteAllAsync<T>(this IClientStreamWriter<T> streamWriter, IEnumerable<T> elements, bool complete = true)
            where T : class
        {
            return WriteAllAsyncImpl<T>(streamWriter, elements, complete);
        }
        private static async PooledTask WriteAllAsyncImpl<T>(IClientStreamWriter<T> streamWriter, IEnumerable<T> elements, bool complete)
            where T : class
        {
            foreach (var element in elements)
            {
                await streamWriter.WriteAsync(element).ConfigureAwait(false);
            }
            if (complete)
            {
                await streamWriter.CompleteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes all elements from given enumerable to the stream.
        /// </summary>
        public static Task WriteAllAsync<T>(this IServerStreamWriter<T> streamWriter, IEnumerable<T> elements)
            where T : class
        {
            return WriteAllAsyncImpl(streamWriter, elements);
        }

        private static async PooledTask WriteAllAsyncImpl<T>(this IServerStreamWriter<T> streamWriter, IEnumerable<T> elements)
            where T : class
        {
            foreach (var element in elements)
            {
                await streamWriter.WriteAsync(element).ConfigureAwait(false);
            }
        }
    }
}
