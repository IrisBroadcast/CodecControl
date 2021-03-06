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
using System.Threading.Tasks;
using CodecControl.Web.HostedServices;
using CodecControl.Web.Models.System;
using Microsoft.AspNetCore.SignalR;

namespace CodecControl.Web.Hub
{
    /// <summary>
    /// Contains methods for the client to call
    /// For methods Server->Client see AudioStatusService
    /// </summary>
    public class AudioStatusHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AudioStatusService _audioStatusService;

        public AudioStatusHub(AudioStatusService audioStatusService)
        {
            _audioStatusService = audioStatusService;
        }

        public override async Task OnConnectedAsync()
        {
            // Do nothing
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _audioStatusService.Unsubscribe(Context.ConnectionId);
            await RemoveFromGroup("SystemStatusListeners");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Audio Status methods
        /// </summary>
        /// <param name="sipAddress"></param>
        /// <returns></returns>
        public async Task Subscribe(string sipAddress)
        {
            await _audioStatusService.Subscribe(Context.ConnectionId, sipAddress);
            await SendOutSystemStatus();
        }

        public void Unsubscribe(string sipAddress = "")
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                _audioStatusService.Unsubscribe(Context.ConnectionId);
            }
            else
            {
                _audioStatusService.Unsubscribe(Context.ConnectionId, sipAddress);
            }
            SendOutSystemStatus();
        }

        /// <summary>
        /// System Status methods
        /// </summary>
        /// <returns></returns>
        public async Task SubscribeSystemStatus()
        {
            await AddToGroup("SystemStatusListeners");
            await SendOutSystemStatus();
        }

        public async Task SendOutSystemStatus()
        {
            var systemStatus = new CodecControlSystemStatus
            {
                Subscriptions = _audioStatusService.Subscriptions
            };
            await Clients.Group("SystemStatusListeners").SendAsync("CodecControlSystemStatus", systemStatus);
        }

    }
}