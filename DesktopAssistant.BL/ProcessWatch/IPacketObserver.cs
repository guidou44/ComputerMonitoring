namespace DesktopAssistant.BL.ProcessWatch
{
    public interface IPacketObserver
    {
        void OnPacketCapture(PacketData data);
    }
}