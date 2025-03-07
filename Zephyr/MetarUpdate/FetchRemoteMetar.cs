using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IO.Compression;
using Zephyr.Entities;
namespace Zephyr.MetarUpdate
{
    /// <summary>
    /// Fetches METAR from a remote server parses the csv into Metar objects.
    /// </summary>
    /// <seealso cref="MetarStorageService" />
    public class FetchRemoteMetar
    {
        private readonly IMetarStorageService _metarStorageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FetchRemoteMetar> _logger;
        private readonly string _metarUrl;

        public FetchRemoteMetar(
            IMetarStorageService metarStorageService,
            IHttpClientFactory httpClientFactory,
            ILogger<FetchRemoteMetar> logger,
            IOptions<MetarUpdateOptions> options)
        {
            _metarStorageService = metarStorageService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _metarUrl = options.Value.SourceUrl;
        }

        public async Task UpdateMetarsAsync()
        {
            try
            {
                var metars = await FetchAndParseMetarsAsync();
                await _metarStorageService.UpdateMetarsAsync(metars);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch METAR data from {Url}", _metarUrl);
                throw new Exception("Failed to fetch METAR data from remote server", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing METAR");
                throw new Exception("Error processing METAR data", ex);
            }
        }

        private async Task<List<Metar>> FetchAndParseMetarsAsync()
        {
            using var client = _httpClientFactory.CreateClient("MetarClient");
            using var response = await client.GetAsync(_metarUrl);
            response.EnsureSuccessStatusCode();

            using var compressed = await response.Content.ReadAsStreamAsync();
            using var decompressor = new GZipStream(compressed, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressor);

            using var csv = new CsvReader(reader, GetCsvConfiguration());
            var metars = new List<Metar>();

            while (await csv.ReadAsync())
            {
                try
                {
                    var metar = ParseMetar(csv);
                    metars.Add(metar);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error parsing METAR row at position {Position}",
                        csv.Context.Parser.RawRow);
                }
            }

            return metars;
        }

        private static CsvConfiguration GetCsvConfiguration() =>
            new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                MissingFieldFound = null,
                Delimiter = ",",
                ShouldSkipRecord = args => args.Row.Parser.RawRow <= 6
            };

        private static Metar ParseMetar(IReaderRow csv) => new()
        {
            MetarText = csv.GetField<string>(0),
            Station = csv.GetField<string>(1),
            ObservationDateTimeZulu = DateTime.ParseExact(csv.GetField<string>(2), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            TemperatureCelsius = GetTemperature(csv.GetField<string>(5))
        };

        private static decimal? GetTemperature(string tempField) =>
            decimal.TryParse(tempField, CultureInfo.InvariantCulture, out var temp) ? temp : null;
    }
}
