using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Service.Interfaces;
using System.Text;

namespace OtePrices.Tests
{
    [TestFixture]
    public class CnbServiceCachedTests
    {
        [Test]
        public void CnbServiceCachedConstructorTest()
        {
            var logger = Substitute.For<ILogger<CnbServiceCached>>();
            var cache = Substitute.For<IDistributedCache>();
            ICnbService? cnbService = null;
            Assert.Throws<ArgumentNullException>(() => new CnbServiceCached(logger, cache, cnbService));
        }

        [TestCase("2023-11-13", "24.45", TestName = "When the date is NOT cached single call to CnbService.")]
        public async Task GetRate_WhenDateIsNotCached_CallsCnbService(DateTime dateTime, decimal expected)
        {
            var logger = Substitute.For<ILogger<CnbServiceCached>>();

            var date = DateOnly.FromDateTime(dateTime);
            var cache = Substitute.For<IDistributedCache>();
            cache.GetAsync(date.ToString())
                .Returns((byte[]?)null);

            var cnbService = Substitute.For<ICnbService>();
            cnbService.GetRate(date).Returns(24.45m);

            var _target = new CnbServiceCached(logger, cache, cnbService);
            decimal result = await _target.GetRate(date);
            Assert.That(result, Is.EqualTo(expected));
            await cnbService.Received(1).GetRate(Arg.Any<DateOnly>());
        }

        [TestCase("2023-11-13", "24.45", TestName = "When the date is cached no call to CnbService.")]
        public async Task GetRate_WhenDateIsCached_NoCallCnbService(DateTime dateTime, decimal expected)
        {
            var logger = Substitute.For<ILogger<CnbServiceCached>>();

            var date = DateOnly.FromDateTime(dateTime);
            var cache = Substitute.For<IDistributedCache>();
            cache.GetAsync(date.ToString())
                .Returns(Encoding.UTF8.GetBytes("24.45"));

            var cnbService = Substitute.For<ICnbService>();

            var _target = new CnbServiceCached(logger, cache, cnbService);
            decimal result = await _target.GetRate(date);
            Assert.That(result, Is.EqualTo(expected));
            await cnbService.DidNotReceive().GetRate(Arg.Any<DateOnly>());
        }
    }
}