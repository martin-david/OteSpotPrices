using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using OteCr;
using Service.Interfaces;
using Service.Model;
using Service.Repository;

namespace OtePrices.Tests
{
    [TestFixture]
    public class OteManagerTests
    {
        [TestCase("2023-11-13", "2023-11-15")]
        public async Task GetOtePricesTest(DateTime startDate, DateTime endDate)
        {
            var date1 = new DateOnly(2023, 11, 13);
            var date2 = new DateOnly(2023, 11, 14);
            var date3 = new DateOnly(2023, 11, 15);

            var cnbService = Substitute.For<ICnbService>();
            cnbService.GetRate(date1).Returns(24.45m);
            cnbService.GetRate(date2).Returns(24.52m);
            cnbService.GetRate(date3).Returns(24.68m);

            var oteCrService = Substitute.For<IOteCrService>();
            oteCrService.GetDamPrice(date1).Returns(Task.FromResult(new GetDamPriceEResponseItem[]
            {
                new () { Price = 75.6m, Hour = 1 },
                new () { Price = 112.3m, Hour = 2 },
                new () { Price = 85.4m, Hour = 3 },
            }));
            oteCrService.GetDamPrice(date2).Returns(Task.FromResult(new GetDamPriceEResponseItem[]
            {
                new () { Price = 60.2m, Hour = 1 },
                new () { Price = 115.4m, Hour = 2 },
                new () { Price = -10.4m, Hour = 3 },
            }));
            oteCrService.GetDamPrice(date3).Returns(Task.FromResult(new GetDamPriceEResponseItem[]
            {
                new () { Price = -104.6m, Hour = 1 },
                new () { Price = 37.8m, Hour = 2 },
                new () { Price = 5.4m, Hour = 3 },
            }));

            var oteManager = new OteManager(cnbService, oteCrService, null);
            var result = await oteManager.GetOtePrices(DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(new List<OtePrice> {
                new(new DateOnly(2023, 11, 13), new TimeOnly(1, 0), 1.85m) { Id = "2023-11-13T01:00:00.000Z" },
                new(new DateOnly(2023, 11, 13), new TimeOnly(2, 0), 2.75m) { Id = "2023-11-13T02:00:00.000Z" },
                new(new DateOnly(2023, 11, 13), new TimeOnly(3, 0), 2.09m) { Id = "2023-11-13T03:00:00.000Z" },
                new(new DateOnly(2023, 11, 14), new TimeOnly(1, 0), 1.48m) { Id = "2023-11-14T01:00:00.000Z" },
                new(new DateOnly(2023, 11, 14), new TimeOnly(2, 0), 2.83m) { Id = "2023-11-14T02:00:00.000Z" },
                new(new DateOnly(2023, 11, 14), new TimeOnly(3, 0), -0.26m) { Id = "2023-11-14T03:00:00.000Z" },
                new(new DateOnly(2023, 11, 15), new TimeOnly(1, 0), -2.58m) { Id = "2023-11-15T01:00:00.000Z" },
                new(new DateOnly(2023, 11, 15), new TimeOnly(2, 0), 0.93m) { Id = "2023-11-15T02:00:00.000Z" },
                new(new DateOnly(2023, 11, 15), new TimeOnly(3, 0), 0.13m) { Id = "2023-11-15T03:00:00.000Z" },
            }));

            await cnbService.Received(1).GetRate(date1);
            await cnbService.Received(1).GetRate(date2);
            await cnbService.Received(1).GetRate(date3);
            await oteCrService.Received(1).GetDamPrice(date1);
            await oteCrService.Received(1).GetDamPrice(date2);
            await oteCrService.Received(1).GetDamPrice(date3);
        }

        [TestCase("2023-11-13")]

        public async Task GetOtePricesTest(DateTime dateTime)
        {
            var date = DateOnly.FromDateTime(dateTime);

            var cnbService = Substitute.For<ICnbService>();
            cnbService.GetRate(date).Returns(24.45m);

            var oteCrService = Substitute.For<IOteCrService>();
            oteCrService.GetDamPrice(date).Returns(Task.FromResult(new GetDamPriceEResponseItem[]
            {
                new () { Price = 756m, Hour = 1 },
                new () { Price = 1123m, Hour = 2 },
                new () { Price = 854m, Hour = 3 },
            }));

            var oteManager = new OteManager(cnbService, oteCrService, null);
            var result = await oteManager.GetOtePrices(date);
            Assert.That(result, Is.Not.Null);
            await cnbService.Received(1).GetRate(date);
            await oteCrService.Received(1).GetDamPrice(date);
        }

        [Test]
        public async Task GetMarketParticipantsTest()
        {
            var oteCrService = Substitute.For<IOteCrService>();
            oteCrService.GetRegisteredMarketParticipants().Returns(Task.FromResult(new GetRutListResponseResult
            {
                Rut = new GetRutListResponseResultRut[]
                {
                    new() { Id = "1", Ean = "123", Company = "123 Company"},
                    new() { Id = "2", Ean = "765456", Company = "John White"},
                    new() { Id = "3", Ean = "654123", Company = "Ian Rich"},
                }
            }));

            var oteManager = new OteManager(null, oteCrService, null);
            await oteManager.GetMarketParticipants();
            await oteCrService.Received(1).GetRegisteredMarketParticipants();
        }

        [Test]
        public async Task LoadOtePricesTest()
        {
            var otePrices = new List<OtePrice>
            {
                new(new DateOnly(2023, 11, 13), new TimeOnly(1, 0), 1.85m)
            };
            var mock = otePrices.BuildMock();

            var otePricesRepository = Substitute.For<IServiceRepository<OtePrice>>();
            otePricesRepository.GetAll().Returns(mock);

            var oteManager = new OteManager(null, null, otePricesRepository);
            var result = await oteManager.LoadOtePrices();
            Assert.That(result, Is.Not.Null);
            Assert.That(result[0].Price, Is.EqualTo(1.85m));
            otePricesRepository.Received(1).GetAll();
        }

        [Test]
        public async Task SaveOtePricesTest()
        {
            var otePrices = new List<OtePrice>
            {
                new(new DateOnly(2023, 11, 13), new TimeOnly(1, 0), 1.85m)
            };

            var otePricesRepository = Substitute.For<IServiceRepository<OtePrice>>();

            var oteManager = new OteManager(null, null, otePricesRepository);
            await oteManager.SaveOtePrices(otePrices);

            await otePricesRepository.Received(1).Create(otePrices);
        }
    }
}