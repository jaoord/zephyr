using Zephyr.Entities;

namespace Zephyr.MetarUpdate
{
    public interface IMetarStorageService
    {
        Task UpdateMetarsAsync(List<Metar> metars);
    }

}
