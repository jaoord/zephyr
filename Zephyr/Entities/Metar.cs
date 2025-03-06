namespace Zephyr.Entities
{
    public class Metar
    {
        public int Id { get; set; }
        public string Station { get; set; }
        public string MetarText { get; set; }
        public decimal? TemperatureCelsius { get; set; }
        public DateTime ObservationDateTimeZulu { get; set; }
    }
}
