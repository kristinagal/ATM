using Newtonsoft.Json;

namespace ATM.FileManager
{
    public class DataService : IDataService //repozitorija
    {
        private readonly string _filePath;

        public DataService(string filePath)
        {
            _filePath = filePath;
        }

        public AtmData LoadATMData()
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<AtmData>(json); //is json sukuria AtmData objekta
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                throw;
            }
        }

        public void SaveATMData(AtmData atmData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(atmData, Formatting.Indented); //is AtmData objekto sufomuoja json string'a
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }
    }
}
