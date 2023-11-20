using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OteCr;

namespace OtePrices.Tests
{
    [TestFixture]
    public class OteCrServiceTests
    {
        private OteCrService _target;
        private PublicDataServiceSoap _publicDataServiceSoap;

        [SetUp]
        public void Setup()
        {
            var logger = Substitute.For<ILogger<OteCrService>>();

            _publicDataServiceSoap = Substitute.For<PublicDataServiceSoap>();

            _target = new OteCrService(logger, _publicDataServiceSoap);
        }

        [Test]
        public async Task GetRegisteredMarketParticipantsTest()
        {
            _ = await _target.GetRegisteredMarketParticipants();
            await _publicDataServiceSoap.Received(1).GetRutListAsync(0, 100000);


        }

        [TestCase("2023-11-13")]
        public async Task GetDamPriceTest(DateTime dateTime)
        {
            var date = DateOnly.FromDateTime(dateTime);
            _ = await _target.GetDamPrice(date);
            await _publicDataServiceSoap.ReceivedWithAnyArgs(1).GetDamPriceEAsync(null);
        }
    }
}