using Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.DbContexts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.UnitTests;

public class TourServiceTests : IClassFixture<TestFixture>
{
    private SqlContext inMemoryDb;
    private ToursService service;
    public TourServiceTests(TestFixture testFixture)
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<ToursService>().Returns(Substitute.For<ILogger<ToursService>>());
        var configuration = Substitute.For<IConfiguration>();

        inMemoryDb = testFixture.DbContext;
        service = new ToursService(loggerFactory, inMemoryDb, configuration);
    }

    [Fact]
    public async Task kjsdflsd()
    {

    }
}