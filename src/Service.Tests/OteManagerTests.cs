using NSubstitute;
using NUnit.Framework;
using OteCr;
using Service.Interfaces;

namespace OtePrices.Tests
{
    [TestFixture]
    public class OteManagerTests
    {
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

            var oteManager = new OteManager(cnbService, oteCrService);
            await oteManager.GetOtePrices(DateOnly.FromDateTime(dateTime));
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

            var oteManager = new OteManager(null, oteCrService);
            await oteManager.GetMarketParticipants();
            await oteCrService.Received(1).GetRegisteredMarketParticipants();
        }
    }
}