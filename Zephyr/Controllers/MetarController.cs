using Microsoft.AspNetCore.Mvc;
using Zephyr.ApiModels;
using Zephyr.Db;

namespace Zephyr.Controllers;

[ApiController]
[Route("[controller]")]
public class MetarController : ControllerBase
{
    private readonly ILogger<MetarController> _logger;
    private readonly ZephyrDbContext _db;

    public MetarController(ILogger<MetarController> logger,
        ZephyrDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet("Latest/{station}")]
    public IActionResult Latest(string station)
    {
        if (string.IsNullOrEmpty(station))
            return BadRequest();

        var metar = _db.Metars
            .Where(m => m.Station == station.Trim().ToUpper())
            .OrderByDescending(m => m.ObservationDateTimeZulu)
            .FirstOrDefault();

        if (metar == null)
            return NotFound();

        return Ok(new MetarModel(metar));
    }


    [HttpGet("Average/{station}")]
    public IActionResult Average(string station)
    {
        if (string.IsNullOrEmpty(station))
            return BadRequest();

        decimal? avgTempC = _db.Metars
                        .Where(m => m.Station == station.Trim().ToUpper())
                        .Where(m => m.ObservationDateTimeZulu > DateTime.UtcNow.AddHours(-24))
                        .Where(m => m.TemperatureCelsius.HasValue)
                        .Average(m => m.TemperatureCelsius);

        if (!avgTempC.HasValue)
            return NotFound();

        return Ok(new AverageTemperatureModel(station, avgTempC.Value));
    }
}
