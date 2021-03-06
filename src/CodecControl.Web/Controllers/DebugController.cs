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

using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
using CodecControl.Web.HostedServices;
using CodecControl.Web.Hub;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("debug")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DebugController : ControllerBase
    {
        private readonly CcmService _ccmService;
        private readonly AudioStatusService _audioStatusService;

        public DebugController(CcmService ccmService, AudioStatusService audioStatusService)
        {
            _ccmService = ccmService;
            _audioStatusService = audioStatusService;
        }

        /// <summary>
        /// Returns all subscriptions to who ever is wondering
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("subscriptions")]
        public List<SubscriptionInfo> Subscriptions()
        {
            return _audioStatusService.Subscriptions;
        }

        /// <summary>
        /// Fetches information about a codec from CCM
        /// </summary>
        /// <param name="sipAddress"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("codecinformation")]
        public async Task<CodecInformation> CodecInformation(string sipAddress)
        {
            return await _ccmService.GetCodecInformationBySipAddress(sipAddress);
        }

    }
}