using Microsoft.EntityFrameworkCore;
using Zephyr.Db;
using Zephyr.Entities;

namespace Zephyr.MetarUpdate
{
    /// <summary>
    /// Stores the fetched METAR into the database.
    /// </summary>
    /// <seealso cref="FetchRemoteMetar" />
    public class MetarStorageService : IMetarStorageService
    {
        private readonly ZephyrDbContext _db;
        private readonly ILogger<MetarStorageService> _logger;

        public MetarStorageService(ZephyrDbContext db, ILogger<MetarStorageService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task UpdateMetarsAsync(List<Metar> metars)
        {
            // cache the latest records for each station based on the highest LastChange
            var existingRecords = await _db.Metars
                .AsNoTracking()
                .GroupBy(m => m.Station)
                .Select(g => g.OrderByDescending(m => m.ObservationDateTimeZulu).First())
                .ToDictionaryAsync(m => new { m.Station, m.ObservationDateTimeZulu });

            var newRecords = new List<Metar>();

            int i = 0;
            foreach (var metar in metars)
            {
                if (!existingRecords.ContainsKey(new { metar.Station, metar.ObservationDateTimeZulu }))
                {
                    newRecords.Add(metar);
                    i++;
                }

                // batch save 
                if (i % 1000 == 0)
                {
                    _db.Metars.AddRange(newRecords);
                    await _db.SaveChangesAsync();
                    newRecords.Clear();
                }
            }

            // any remaining records
            if (newRecords.Any())
            {
                _db.Metars.AddRange(newRecords);
                await _db.SaveChangesAsync();
            }
        }
    }
}
