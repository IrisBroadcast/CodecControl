﻿using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetVumetersResponse: IkusNetStatusResponseBase
    {

        public int ProgramTxLeft { get; private set; }
        public int ProgramTxRight { get; private set; }
        public int ProgramRxLeft { get; private set; }
        public int ProgramRxRight { get; private set; }
        public int TalkbackTxLeft { get; private set; }
        public int TalkbackTxRight { get; private set; }
        public int TalkbackRxLeft { get; private set; }
        public int TalkbackRxRight { get; private set; }

        public IkusNetGetVumetersResponse(SocketProxy socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetVumeters, 32);
            ProgramTxLeft = (int) ConvertHelper.DecodeUInt(responseBytes, 0);
            ProgramTxRight = (int) ConvertHelper.DecodeUInt(responseBytes, 4);
            ProgramRxLeft = (int) ConvertHelper.DecodeUInt(responseBytes, 8);
            ProgramRxRight = (int) ConvertHelper.DecodeUInt(responseBytes, 12);
        }

    }

  
}