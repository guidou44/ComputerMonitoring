namespace DesktopAssistant.BL.Persistence
{
    public interface IRepository
    {
        TObject Read<TObject>();

        void Update<TObject>(TObject updatedEntity);

    }
}