namespace Zephyr.ApiModels
{
    public class AverageTemperatureModel
    {
        public AverageTemperatureModel(string station, decimal temp)
        {
            Station = station.Trim().ToUpper();
            Average24hTemperature = Math.Round(temp, 1);
        }

        public string Station { get; set; }
        public decimal Average24hTemperature { get; set; }
    }
}
