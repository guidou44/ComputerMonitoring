using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.HardwareAccessTests.Models
{
    public class ComputerResourceTest
    {
        [Fact]
        public void GivenValidRemoteResource_WhenPing_ItReturnsTrue()
        {
            ComputerResource subject = new ComputerResource()
            {
                IsRemote = true,
                RemoteIp = "localhost"
            };

            Assert.True(subject.TryPing());
        }

        [Fact]
        public void GivenValidRemoteResourceNotRemoteFlag_WhenPing_ItReturnsFalse()
        {
            ComputerResource subject = new ComputerResource()
            {
                IsRemote = false,
                RemoteIp = "localhost"
            };

            Assert.False(subject.TryPing());
        }

        [Fact]
        public void GivenInvalidRemoteResource_WhenPing_ItReturnsFalse()
        {
            ComputerResource subject = new ComputerResource()
            {
                IsRemote = true,
                RemoteIp = "ABCD"
            };

            Assert.False(subject.TryPing());
        }
    }
}
