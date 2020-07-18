using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.DesktopAssistantTests.ViewModels.Fixtures
{
    public class WindowFixture : IRelocatable, ITopMost
    {
        public double ActualHeight { get; set; }
        public double ActualWidth { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public bool Topmost { get; set; }
    }
}
