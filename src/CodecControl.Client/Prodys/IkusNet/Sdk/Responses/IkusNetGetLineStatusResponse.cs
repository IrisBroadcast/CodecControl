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

using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetLineStatusResponse : IkusNetStatusResponseBase
    {
        public IkusNetGetLineStatusResponse(SocketProxy socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetLineStatus, 268);
            Address = responseBytes.ToNullTerminatedString(0, 256);
            LineStatus = (IkusNetLineStatus)ConvertHelper.DecodeUInt(responseBytes, 256);
            DisconnectionCode = (IkusNetStreamingDisconnectionReason)ConvertHelper.DecodeUInt(responseBytes, 260);
            IpCallType = (IkusNetIPCallType)ConvertHelper.DecodeUInt(responseBytes, 264);

        }

        public string Address { get; }
        public IkusNetLineStatus LineStatus { get; }
        public IkusNetStreamingDisconnectionReason DisconnectionCode { get; }
        public IkusNetIPCallType IpCallType { get; }

    }
}