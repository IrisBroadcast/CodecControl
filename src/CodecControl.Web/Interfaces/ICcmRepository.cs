﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;

namespace CodecControl.Web.CCM
{
    public interface ICcmRepository
    {
        Task<List<CodecInformation>> GetCodecInformationList();
    }
}