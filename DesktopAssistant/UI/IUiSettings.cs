namespace DesktopAssistant.UI
{
    public interface IUiSettings
    {
        string Main { get; set; }
        string FontMain { get; set; }
        string BackGroundMain { get; set; }
        string BackGroundSecondary { get; set; }
         double BackGroundMainOpacity { get; set; }
        double BackGroundSecondaryOpacity { get; set; }
        string ProgressBar { get; set; }
        string ProgressBarBackGround { get; set; }
    }
}