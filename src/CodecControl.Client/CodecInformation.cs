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
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using Newtonsoft.Json;

namespace CodecControl.Client
{
    public class CodecInformation
    {
        public string SipAddress { get; set; }
        public string Ip { get; set; }
        public string Api { get; set; }
        public string GpoNames { get; set; }
        public int NrOfInputs { get; set; }
        public int NrOfGpos { get; set; }

        [JsonIgnore]
        public Type CodecApiType
        {
            get
            {
                if (Enum.TryParse(Api, true, out CodecApiTypes apiType))
                {
                    switch (apiType)
                    {
                        case CodecApiTypes.IkusNet:
                            return typeof(IkusNetApi);
                        case CodecApiTypes.IkusNetSt:
                            return typeof(IkusNetStApi);
                        case CodecApiTypes.BareSipRest:
                            return typeof(BaresipRestApi);
                        default:
                            // TODO: Log as warning
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}