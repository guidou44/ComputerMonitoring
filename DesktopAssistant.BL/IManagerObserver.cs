using System;

namespace DesktopAssistant.BL
{
    public interface IManagerObserver
    {
        void OnHardwareInfoChange();
        void OnProcessWatchInfoChange();
        void OnError(Exception e);
    }
}