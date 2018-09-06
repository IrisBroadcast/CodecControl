﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    public class ProdysSocket : Socket {

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static TimeSpan _lingerTimeSpan = new TimeSpan(0,0,0,30);

        public string IpAddress { get; }
        private DateTime _evictionTime = DateTime.Now.Add(_lingerTimeSpan);

        public ProdysSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string ipAddress) : base(addressFamily, socketType, protocolType)
        {
            IpAddress = ipAddress;
        }

        public ProdysSocket(SocketInformation socketInformation, string ipAddress) : base(socketInformation)
        {
            IpAddress = ipAddress;
        }

        public ProdysSocket(SocketType socketType, ProtocolType protocolType, string ipAddress) : base(socketType, protocolType)
        {
            IpAddress = ipAddress;
        }

        public void UpdateEvictionTime()
        {
            _evictionTime = DateTime.Now.Add(_lingerTimeSpan);
        }

        public bool IsOld()
        {
            return DateTime.Now > _evictionTime;
        }

        public static async Task<ProdysSocket> GetConnectedSocketAsync(string address, int sendTimeout = 300)
        {
            // TODO: Hämta socket ur poolen om sådan finns. Markera som tagen.
            // TODO: Om det inte finns socket skapa ny och lägg i poolen.

            // TODO: Kolla om redan uppkopplad socket finns. Om det finns returnera den.
            // TODO: Om socket saknas, gör uppkoppling, spara i lista med sockets, returnera.

            // TODO: Efter x sekunder utan användning ska socketen stängas 

            IPAddress ipAddress = GetIpAddress(address);
            if (ipAddress == null)
            {
                log.Warn($"Unable to resolve ip address for {address}");
                throw new UnableToResolveAddressException();
            }

            // Try with authenticated connect first
            // INFO: It seems that authenticated connect works also when authentication is not active on the codec. At least on some firmware versions...
            ProdysSocket connectedSocket = await ConnectAsync(ipAddress, new CsConnect2(), sendTimeout);

            if (connectedSocket != null)
            {
                return connectedSocket;
            }

            log.Warn("Unable to connect to codec at {0} using authenticated connect.", ipAddress);

            // Otherwise, try non authenticated connect
            connectedSocket = await ConnectAsync(ipAddress, new CsConnect(), sendTimeout);

            if (connectedSocket != null)
            {
                return connectedSocket;
            }

            log.Warn("Unable to connect to codec at {0}. Both authenticated and unauthenticated connect failed.", ipAddress);
            throw new UnableToConnectException();
        }

        private static IPAddress GetIpAddress(string address)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                return ipAddress;
            }
            var ips = Dns.GetHostAddresses(address);
            return ips.Length > 0 ? ips[0] : null;
        }


        private static async Task<ProdysSocket> ConnectAsync(IPAddress ipAddress, ConnectCommandBase connectCmd, int sendTimeout)
        {
            ProdysSocket socket = null;

            try
            {
                socket = new ProdysSocket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP, ipAddress.ToString());

                if (sendTimeout > 0)
                {
                    socket.SendTimeout = sendTimeout;
                }

                var endpoint = new IPEndPoint(ipAddress, Sdk.IkusNet.ExternalProtocolIpCommandsPort);

                await socket.ConnectAsync(endpoint); // TODO: timeout ?

                if (!socket.Connected)
                {
                    socket.Close();
                    return null;
                }

                var bytes = connectCmd.GetBytes();
                var sent = socket.Send(bytes);

                if (sent <= 0 || !socket.Connected)
                {
                    socket.Close();
                    return null;
                }

                // Read response
                var buffer = new byte[16];
                socket.Receive(buffer);

                var command = (Command)ConvertHelper.DecodeUInt(buffer, 0);
                var length = (int)ConvertHelper.DecodeUInt(buffer, 4);
                var receivedCommand = (Command)ConvertHelper.DecodeUInt(buffer, 8);
                // TODO: Verify command, length, receivedCommand
                var acknowleged = Convert.ToBoolean(ConvertHelper.DecodeUInt(buffer, 12));

                log.Debug("Connect response from codec at {0}: {1}", ipAddress, acknowleged);

                if (!acknowleged)
                {
                    socket.Close();
                    socket.Dispose();
                    return null;
                }

                return socket;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when connecting to codec at {0}", ipAddress);
                socket?.Close();
                return null;
            }
        }
    }
}