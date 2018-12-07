﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Exceptions;
using CodecControl.Web.CCM;
using CodecControl.Web.Models.Requests;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CodecControl.Web.Controllers.Base
{
    public class CodecControlControllerBase : ApiControllerBase
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected readonly IServiceProvider _serviceProvider;
        protected readonly CcmService _ccmService;

        public CodecControlControllerBase(CcmService ccmService, IServiceProvider serviceProvider)
        {
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }

        protected async Task<ActionResult<TResult>> Execute<TResult>(string sipAddress, Func<ICodecApi, CodecInformation, Task<TResult>> func)
        {
            using (new TimeMeasurer("CodecControl"))
            {
                try
                {

                    if (string.IsNullOrEmpty(sipAddress))
                    {
                        log.Info($"Invalid request. Missing SIP address in request: {Request.GetDisplayUrl()} " +
                                 $"[Host={Request.Headers["Host"]}, UserAgent={Request.Headers["User-Agent"]}]");
                        return BadRequest();
                    }

                    var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);

                    if (codecInformation == null)
                    {
                        log.Info($"Codec {sipAddress} is not currently registered in CCM.");
                        return CodecUnavailable();
                    }

                    var codecApiType = codecInformation.CodecApiType;
                    var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

                    if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
                    {
                        log.Info($"Missing information to connect to codec {sipAddress}");
                        return CodecUnavailable();
                    }

                    log.Debug($"Sending codec control command to {sipAddress} on IP {codecInformation.Ip} using API {codecInformation.Api}");
                    using (new TimeMeasurer("Make Codec Request"))
                    {
                        return await func(codecApi, codecInformation);
                    }
                }
                catch (CodecInvocationException ex)
                {
                    // When response from codec was unparsable or indicates that request was unsuccessful.
                    log.Info($"Error when sending codec control command to {sipAddress}. {ex.Message}");
                    return InternalServerError();
                }
                catch (CodecException ex)
                {
                    log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                    return InternalServerError();
                }
                catch (Exception ex)
                {
                    log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                    return InternalServerError();
                }
            }
        }

    }
}