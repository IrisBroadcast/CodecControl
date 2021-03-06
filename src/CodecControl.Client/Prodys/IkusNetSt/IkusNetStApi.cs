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
using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;

namespace CodecControl.Client.Prodys.IkusNetSt
{
    /// <summary>
    /// This API is intended for Quantum ST that lacks controllable inputs.
    /// </summary>
    public class IkusNetStApi : IkusNetApiBase, ICodecApi
    {

        public IkusNetStApi(ProdysSocketPool prodysSocketPool) : base(prodysSocketPool)
        {
        }

        public async Task<bool> CheckIfAvailableAsync(string ip)
        {
            try
            {
                using (var socket = await ProdysSocketPool.TakeSocket(ip))
                {
                    // Send dummy command to codec.
                    SendCommand(socket, new CommandIkusNetGetVuMeters());
                    var dummResponse = new IkusNetGetVumetersResponse(socket);
                    return true; // Success
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Get Commands

        // For test only
        public async Task<string> GetDeviceNameAsync(string hostAddress)
        {
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetSysGetDeviceName());
                var response = new IkusNetGetDeviceNameResponse(socket);
                return response.DeviceName;
            }
        }

        public async Task<bool?> GetGpiAsync(string hostAddress, int gpio)
        {
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpi { Gpio = gpio });
                var response = new IkusNetGetGpiResponse(socket);
                return response.Active;
            }
        }

        public async Task<bool?> GetGpoAsync(string hostAddress, int gpio)
        {
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpio });
                var response = new IkusNetGetGpoResponse(socket);
                return response.Active;
            }
        }

        public virtual Task<bool> GetInputEnabledAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<int> GetInputGainLevelAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<(bool, int)> GetInputGainAndStatusAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public async Task<LineStatus> GetLineStatusAsync(string hostAddress, string lineEncoder = "ProgramL1")
        {
            // TODO: Get actual amount of lines from CCM and add to CodecInformation object to determine if it exists
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                // If the device have multiple lines and encoders
                var deviceLineEncoder = string.IsNullOrEmpty(lineEncoder) || lineEncoder == "null" ? "ProgramL1" : lineEncoder;
                IkusNetLine statusSelectedLineEncoder = IkusNetLine.ProgramL1;
                try
                {
                    statusSelectedLineEncoder = (IkusNetLine)Enum.Parse(typeof(IkusNetLine), deviceLineEncoder, true);
                }
                catch (Exception)
                {
                    statusSelectedLineEncoder = IkusNetLine.ProgramL1;
                }

                log.Debug($"Trying to GetLineStatus statusSelectedLine: {statusSelectedLineEncoder} , hostAddress: {hostAddress}");

                SendCommand(socket, new CommandIkusNetGetLineStatus
                {
                    Line = statusSelectedLineEncoder
                });

                var response = new IkusNetGetLineStatusResponse(socket);
                
                return new LineStatus
                {
                    LineEncoder = statusSelectedLineEncoder.ToString(),
                    StatusCode = IkusNetMapper.MapToLineStatus(response.LineStatus),
                    DisconnectReason = IkusNetMapper.MapToDisconnectReason(response.DisconnectionCode),
                    RemoteAddress = response.Address
                };
            }
        }

        public async Task<VuValues> GetVuValuesAsync(string hostAddress)
        {
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var response = new IkusNetGetVumetersResponse(socket);
                return IkusNetMapper.MapToVuValues(response);
            }
        }

        public async Task<AudioMode> GetAudioModeAsync(string hostAddress)
        {
            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                // Get encoder algoritm
                SendCommand(socket, new CommandIkusNetGetEncoderAudioMode());
                var encoderResponse = IkusNetGetEncoderAudioModeResponse.GetResponse(socket);

                // Get decoder algoritm
                SendCommand(socket, new CommandIkusNetGetDecoderAudioMode());
                var decoderResponse = IkusNetGetDecoderAudioModeResponse.GetResponse(socket);

                return new AudioMode
                {
                    EncoderAudioAlgoritm = IkusNetMapper.MapToAudioAlgorithm(encoderResponse.AudioAlgorithm),
                    DecoderAudioAlgoritm = IkusNetMapper.MapToAudioAlgorithm(decoderResponse.AudioAlgorithm)
                };
            }
        }

        public virtual async Task<AudioStatus> GetAudioStatusAsync(string hostAddress, int nrOfInputs, int nrOfGpos)
        {
            var audioStatus = new AudioStatus();

            using (var socket = await ProdysSocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var vuResponse = new IkusNetGetVumetersResponse(socket);
                audioStatus.VuValues = IkusNetMapper.MapToVuValues(vuResponse);

                // // Input status not implemented in Quantum ST
                audioStatus.InputStatus = new List<InputStatus>();

                audioStatus.Gpos = new List<GpoStatus>();

                for (int gpo = 0; gpo < nrOfGpos; gpo++)
                {
                    SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpo });
                    var response = new IkusNetGetGpoResponse(socket);
                    var gpoEnable = response.Active;
                    if (!gpoEnable.HasValue)
                    {
                        // Indication of missing GPO for the number. Probably we passed the last one.
                        break;
                    }
                    audioStatus.Gpos.Add(new GpoStatus() { Index = gpo, Active = gpoEnable.Value });
                }
            }

            return audioStatus;
        }

        #endregion

        #region Configuration Commands
        public async Task<bool> CallAsync(string hostAddress, string callee, string profileName, string deviceEncoder = "Program")
        {
            // TODO: first check codec call status. Do not execute the call method if the codec is already in a call.
            // Some codecs will hangup the current call and dial up the new call without hesitation.

            // If the device have multiple encoders to call with
            var deviceLineEncoder = string.IsNullOrEmpty(deviceEncoder) || deviceEncoder == "null" ? "Program" : deviceEncoder;
            IkusNetCodec callSelectedEncoder = IkusNetCodec.Program;
            try
            {
                callSelectedEncoder = (IkusNetCodec)Enum.Parse(typeof(IkusNetCodec), deviceLineEncoder, true);
            }
            catch (Exception)
            {
                callSelectedEncoder = IkusNetCodec.Program;
            }

            var cmd = new CommandIkusNetCall
            {
                Address = callee,
                Profile = profileName,
                CallContent = IkusNetCallContent.Audio,
                CallType = IkusNetIPCallType.UnicastBidirectional,
                Codec = callSelectedEncoder,
            };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> HangUpAsync(string hostAddress, string deviceEncoder = "Program")
        {
            // If the device have multiple encoders to hang up
            var deviceLineEncoder = string.IsNullOrEmpty(deviceEncoder) || deviceEncoder == "null" ? "Program" : deviceEncoder;
            IkusNetCodec hangupSelectedEncoder = IkusNetCodec.Program;
            try
            {
                hangupSelectedEncoder = (IkusNetCodec)Enum.Parse(typeof(IkusNetCodec), deviceLineEncoder, true);
            }
            catch (Exception)
            {
                hangupSelectedEncoder = IkusNetCodec.Program;
            }

            var cmd = new CommandIkusNetHangUp { Codec = hangupSelectedEncoder };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> RebootAsync(string hostAddress)
        {
            var cmd = new CommandIkusNetReboot();
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> SetGpoAsync(string hostAddress, int gpo, bool active)
        {
            var cmd = new CommandIkusNetSetGpo { Active = active, Gpo = gpo };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public virtual Task<bool> SetInputEnabledAsync(string hostAddress, int input, bool enabled)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<int> SetInputGainLevelAsync(string hostAddress, int input, int gainLevel)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }
        #endregion

    }
}