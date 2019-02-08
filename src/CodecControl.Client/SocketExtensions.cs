﻿#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CodecControl.Client
{
    public static class SocketTaskExtensions
    {
        private const int ConnectionTimedOutStatusCode = 10060;

        /// <summary>
        /// Connects the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="endpoint">The IP endpoint.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public static async Task ConnectAsync(this Socket socket, EndPoint endpoint, int timeout)
        {
            TimeSpan timeOut = TimeSpan.FromMilliseconds(timeout);

            var cancellationCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    var task = socket.ConnectAsync(endpoint);

                    using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
                    {
                        if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                        {
                            throw new OperationCanceledException(cts.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                socket.Close();
                throw new SocketException(ConnectionTimedOutStatusCode);
            }
        }

        public static void Connect(this Socket socket, EndPoint endpoint, int timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (socket.Connected)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new SocketException(ConnectionTimedOutStatusCode);
            }
        }

    }
}
