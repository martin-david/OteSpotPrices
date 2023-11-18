using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Service.Tests;
using System.Net;
using System.Net.Http;

namespace OtePrices.Tests
{
    [TestFixture]
    public class CnbServiceTests
    {
        private CnbService _target;

        [SetUp]
        public void Setup()
        {
            var logger = Substitute.For<ILogger<CnbService>>();

            var httpMessageHandler = new MockHttpMessageHandler("16.11.2023 #222\r\nzemě|měna|množství|kód|kurz\r\nAustrálie|dolar|1|AUD|14,613\r\nBrazílie|real|1|BRL|4,641\r\nBulharsko|lev|1|BGN|12,501\r\nČína|žen-min-pi|1|CNY|3,110\r\nDánsko|koruna|1|DKK|3,278\r\nEMU|euro|1|EUR|24,450\r\nFilipíny|peso|100|PHP|40,472\r\nHongkong|dolar|1|HKD|2,890\r\nIndie|rupie|100|INR|27,084\r\nIndonesie|rupie|1000|IDR|1,450\r\nIsland|koruna|100|ISK|15,949\r\nIzrael|nový šekel|1|ILS|5,974\r\nJaponsko|jen|100|JPY|14,914\r\nJižní Afrika|rand|1|ZAR|1,231\r\nKanada|dolar|1|CAD|16,425\r\nKorejská republika|won|100|KRW|1,742\r\nMaďarsko|forint|100|HUF|6,498\r\nMalajsie|ringgit|1|MYR|4,809\r\nMexiko|peso|1|MXN|1,305\r\nMMF|ZPČ|1|XDR|29,905\r\nNorsko|koruna|1|NOK|2,077\r\nNový Zéland|dolar|1|NZD|13,482\r\nPolsko|zlotý|1|PLN|5,593\r\nRumunsko|leu|1|RON|4,919\r\nSingapur|dolar|1|SGD|16,713\r\nŠvédsko|koruna|1|SEK|2,128\r\nŠvýcarsko|frank|1|CHF|25,341\r\nThajsko|baht|100|THB|63,556\r\nTurecko|lira|1|TRY|0,786\r\nUSA|dolar|1|USD|22,547\r\nVelká Británie|libra|1|GBP|27,945\r\n", HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(httpClient);

            _target = new CnbService(logger, httpClientFactory.CreateClient());
        }

        [TestCase("2023-11-13", "24.45")]
        public async Task GetRate_WhenCorrectDate_ReturnsRate(DateTime date, decimal expected)
        {
            decimal result = await _target.GetRate(DateOnly.FromDateTime(date));
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}