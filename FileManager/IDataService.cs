namespace ATM.FileManager
{
    public interface IDataService
    {
        AtmData LoadATMData();
        void SaveATMData(AtmData atmData);
    }
}