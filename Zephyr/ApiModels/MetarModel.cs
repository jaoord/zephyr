using Zephyr.Entities;

namespace Zephyr.ApiModels
{
    public class MetarModel
    {
        public MetarModel() { }
        public MetarModel(Metar metar)
        {
            Station = metar.Station;
            MetarText = metar.MetarText;
            ObservationDateTimeZulu = metar.ObservationDateTimeZulu;
            TemperatureCelsius = metar.TemperatureCelsius;
        }

        public string Station { get; set; }
        public string MetarText { get; set; }
        public DateTime ObservationDateTimeZulu { get; set; }
        public decimal? TemperatureCelsius { get; set; }

    }
}
