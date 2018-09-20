﻿using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using Xunit;

namespace CodecControl.IntegrationTests.Baresip
{
    public class BaresipApiTests
    {
        private readonly string _ip;

        public BaresipApiTests()
        {
            _ip = "134.25.127.231";

        }

        [Fact]
        public async Task IsAvailable()
        {
            var sut = new BaresipRestApi();
            var success = await sut.CheckIfAvailableAsync(_ip);
            Assert.True(success);
        }

        [Fact]
        public async Task SetInputEnabled()
        {
            var sut = new BaresipRestApi();
            var success = await sut.SetInputEnabledAsync(_ip, 0, true);
            Assert.True(success);
        }

        [Fact]
        public async Task GetInputLevel()
        {
            var sut = new IkusNetApi(new SocketPool());

            await sut.SetInputGainLevelAsync(_ip, 0, 6);

            var level = await sut.GetInputGainLevelAsync(_ip, 0);
            Assert.Equal(6, level);

            await sut.SetInputGainLevelAsync(_ip, 0, 4);

            level = await sut.GetInputGainLevelAsync(_ip, 0);
            Assert.Equal(4, level);

        }

        [Fact]
        public async Task GetLineStatus()
        {
            var sut = new IkusNetApi(new SocketPool());

            LineStatus lineStatus = await sut.GetLineStatusAsync(_ip);
            Assert.Equal(LineStatusCode.NoPhysicalLine, lineStatus.StatusCode);
            Assert.Equal(DisconnectReason.None, lineStatus.DisconnectReason);
        }

        [Fact(Skip = "To avoid unintentional calling")]
        public async Task Call()
        {
            var sut = new IkusNetApi(new SocketPool());

            var callee = "sto-s17-01@acip.example.com";
            var profileName = "Studio";

            bool result = await sut.CallAsync(_ip, callee, profileName);
            Assert.True(result);
        }

        [Fact(Skip = "To avoid unintentional hangup")]
        public async Task Hangup()
        {
            var sut = new IkusNetApi(new SocketPool());
            bool result = await sut.HangUpAsync(_ip);
            Assert.True(result);
        }
    }
}
