﻿using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetPresetLoad : CommandBase
    {
        public CommandIkusNetPresetLoad() : base(Command.IkusNetPresetLoad, 256)
        {
        }

        public string PresetToLoad { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeString(PresetToLoad, bytes, offset, 256);

        }
    }

}