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
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    /// <summary>
    /// Encapsulate a ProdysSocket
    /// when a Dispose happens, the socket is returned to the pool
    /// </summary>
    public class SocketProxy : IDisposable
    {
        private readonly ProdysSocket _socket;
        private readonly SocketPool _socketPool;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public SocketProxy(ProdysSocket socket, SocketPool socketPool)
        {
            _socket = socket;
            _socketPool = socketPool;
        }

        public int Send(byte[] buffer)
        {
            return _socket.Send(buffer);
        }

        public int Receive(byte[] buffer)
        {
            return _socket.Receive(buffer);
        }
        
        public void Dispose()
        {
            // Return the socket-instance to the pool, but never close the socket
            _socketPool.ReleaseSocket(_socket);
        }
    }
}