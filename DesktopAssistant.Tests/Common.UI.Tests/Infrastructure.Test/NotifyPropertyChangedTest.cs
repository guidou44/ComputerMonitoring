using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Tests.Common.UI.Tests.Infrastructure.Test.Fixtures;
using Xunit;

namespace DesktopAssistant.Tests.Common.UI.Tests.Infrastructure.Test
{
    public class NotifyPropertyChangedTest
    { 

        [Fact]
        public void GivenProperty_WhenRaisingPropertyChanged_ThenItRaises3TimesForPropertyName()
        {
            NotifyPropertyChangedFixture eventRaiserSubject = new NotifyPropertyChangedFixture();
            List<string> firedEvents = new List<string>();
            eventRaiserSubject.PropertyChanged += ((sender, e) => firedEvents.Add(e.PropertyName));

            eventRaiserSubject.ModifiedVariable = "4";

            Assert.Equal(3, firedEvents.Count);
            Assert.Contains("1", firedEvents);
            Assert.Contains("2", firedEvents);
            Assert.Contains("3", firedEvents);
        }
    }
}
