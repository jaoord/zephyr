using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zephyr.ApiModels;
using Zephyr.Controllers;
using Zephyr.Db;
using Zephyr.Entities;

namespace Zephyr.Tests
{
    [TestClass]
    public sealed class MetarControllerTests
    {
        private ZephyrDbContext _context = null!;
        private Mock<ILogger<MetarController>> _loggerMock = null!;
        private MetarController _controller = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ZephyrDbContext>()
                .UseInMemoryDatabase(databaseName: "ZephyrTest")
                .Options;

            _context = new ZephyrDbContext(options);

            SeedDatabase(_context);

            _loggerMock = new Mock<ILogger<MetarController>>();
            _controller = new MetarController(_loggerMock.Object, _context);
        }

        private void SeedDatabase(ZephyrDbContext context)
        {
            context.Metars.AddRange(new List<Metar>
            {
                new Metar { Station = "EHGG", MetarText = "METAR EHGG 1", ObservationDateTimeZulu = DateTime.UtcNow.AddHours(-1), TemperatureCelsius = 10 },
                new Metar { Station = "EHGG", MetarText = "METAR EHGG 2", ObservationDateTimeZulu = DateTime.UtcNow, TemperatureCelsius = 20 }
            });
            context.SaveChanges();
        }

        [TestMethod]
        [DoNotParallelize()]
        public void Latest_StationFound_ReturnsLatestMetar()
        {
            var result = _controller.Latest("EHGG") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var metar = result.Value as MetarModel;
            Assert.IsNotNull(metar);
            Assert.AreEqual("METAR EHGG 2", metar.MetarText);
        }

        [TestMethod]
        public void Latest_StationNull_ReturnsBadRequest()
        {
            var result = _controller.Latest(null) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [TestMethod]
        public void Latest_InvalidStation_ReturnsNotFound()
        {
            var result = _controller.Latest("ABCD") as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        public void Average_StationFound_ReturnsAverageTemp()
        {
            var result = _controller.Average("EHGG") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var tempC = result.Value as AverageTemperatureModel;
            Assert.IsNotNull(tempC);
            Assert.AreEqual(15, tempC.Average24hTemperature);
        }

        [TestMethod]
        public void Average_StationNull_ReturnsBadRequest()
        {
            var result = _controller.Average(null) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }


        [TestMethod]
        public void Average_InvalidStation_ReturnsNotFound()
        {
            var result = _controller.Average("ABCD") as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        [DoNotParallelize()]
        public void Average_NullTemperature_ExcludesFromAverage()
        {
            _context.Metars.Add(new Metar { Station = "EHGG", MetarText = "METAR EHGG 3", ObservationDateTimeZulu = DateTime.UtcNow, TemperatureCelsius = null });
            _context.SaveChanges();

            var result = _controller.Average("EHGG") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var tempC = result.Value as AverageTemperatureModel;
            Assert.IsNotNull(tempC);
            Assert.AreEqual(15, tempC.Average24hTemperature);
        }
    }
}
