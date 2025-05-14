using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;
using System.Text.Json;

namespace Tests;

public class TestFixture : IDisposable
{
    public SqlContext DbContext { get; private set; }
    public TestFixture()
    {
        var options = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        DbContext = new SqlContext(options);

        // Add test data
        var accounts = new List<Account>
        {
            new Account 
            { 
                Id = 1,
                AccountId = "test_guide1",
                Login = "guide1@test.com",
                Username = "guide1@test.com",
                PasswordHash = "dummy_hash",
                UtcCreated = DateTime.UtcNow.AddDays(-100),
                Roles = AccountRolesEnum.Guide,
                Locale = LocalizationCode.ENG,
                AvatarUrl = "https://test.com/avatar1.jpg",
                IsDeleted = false
            },
            new Account 
            { 
                Id = 2,
                AccountId = "test_guide2",
                Login = "guide2@test.com",
                Username = "guide2@test.com",
                PasswordHash = "dummy_hash",
                UtcCreated = DateTime.UtcNow.AddDays(-90),
                Roles = AccountRolesEnum.Guide,
                Locale = LocalizationCode.ENG,
                AvatarUrl = "https://test.com/avatar2.jpg",
                IsDeleted = false
            },
            new Account 
            { 
                Id = 3,
                AccountId = "test_user1",
                Login = "user1@test.com",
                Username = "user1@test.com",
                PasswordHash = "dummy_hash",
                UtcCreated = DateTime.UtcNow.AddDays(-50),
                Roles = AccountRolesEnum.Client,
                Locale = LocalizationCode.ENG,
                AvatarUrl = "https://test.com/avatar3.jpg",
                IsDeleted = false
            }
        };

        var guides = new List<Guide>
        {
            new Guide 
            { 
                Id = 1,
                AccountId = accounts[0].Id,
                Account = accounts[0],
                Name = "John",
                Surname = "Doe",
                Age = 35,
                CareerStartUtc = DateTime.UtcNow.AddYears(-5),
                IsActive = true
            },
            new Guide 
            { 
                Id = 2,
                AccountId = accounts[1].Id,
                Account = accounts[1],
                Name = "Jane",
                Surname = "Smith",
                Age = 28,
                CareerStartUtc = DateTime.UtcNow.AddYears(-3),
                IsActive = true
            }
        };

        var tours = new List<Tour>
        {
            new Tour
            {
                Id = 1,
                GuideId = guides[0].Id,
                Guide = guides[0],
                Title = "Historical City Tour",
                Description = "Explore the rich history of our city",
                ImageUrl = "https://test.com/tour1.jpg",
                Locations = new List<int> { 1, 2 },
                Price = 100.00m,
                TourType = TourTypeEnum.Hiking,
                WithGuide = true,
                SpecialOffers = SpecialOfferEnum.OnSale,
                DurationDays = 3,
                Classification = TourClassificationEnum.Group,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastModifiedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Tour
            {
                Id = 2,
                GuideId = guides[1].Id,
                Guide = guides[1],
                Title = "Nature Adventure",
                Description = "Experience the beauty of nature",
                ImageUrl = "https://test.com/tour2.jpg",
                Locations = new List<int> { 3, 4, 5 },
                Price = 150.00m,
                TourType = TourTypeEnum.Recovery,
                WithGuide = true,
                SpecialOffers = SpecialOfferEnum.SpecialDiscount,
                DurationDays = 5,
                Classification = TourClassificationEnum.Private,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                LastModifiedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        var tourInstances = new List<TourInstance>
        {
            new TourInstance
            {
                Id = 1,
                TourId = tours[0].Id,
                Tour = tours[0],
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(10),
                MaxParticipants = 20,
                IsCancelled = false
            },
            new TourInstance
            {
                Id = 2,
                TourId = tours[1].Id,
                Tour = tours[1],
                StartDate = DateTime.UtcNow.AddDays(14),
                EndDate = DateTime.UtcNow.AddDays(19),
                MaxParticipants = 10,
                IsCancelled = false
            },
            new TourInstance
            {
                Id = 3,
                TourId = tours[0].Id,
                Tour = tours[0],
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(33),
                MaxParticipants = 15,
                IsCancelled = false
            }
        };

        // Add rates for some instances
        var rates = new List<TourInstanceRate>
        {
            new TourInstanceRate
            {
                Id = 1,
                TourInstanceId = tourInstances[0].Id,
                TourInstance = tourInstances[0],
                TouristAccountId = accounts[2].Id,
                TouristAccount = accounts[2],
                Rate = 50, // 5.0 rating
                TouristCommentary = "Excellent tour!",
                RatedTimeUtc = DateTime.UtcNow.AddDays(-1)
            }
        };

        // Add bookings for some instances
        var bookings = new List<TourBooking>
        {
            new TourBooking
            {
                Id = 1,
                TourInstanceId = tourInstances[0].Id,
                TourInstance = tourInstances[0],
                AccountId = accounts[2].Id,
                Account = accounts[2],
                BookedUtc = DateTime.UtcNow.AddDays(-5),
                TotalPrice = 100.00m
            }
        };

        // Add test languages with localizations
        var languages = new List<Language>
        {
            new Language(LocalizationCode.ENG, JsonSerializer.Serialize(new List<LanguageLocationsDto>
            {
                new(1, "Kyiv", "Ukraine"),
                new(2, "Lviv", "Ukraine"),
                new(3, "New York", "United States"),
                new(4, "Los Angeles", "United States"),
                new(5, "London", "United Kingdom")
            }), JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Tour_Details_Title", "Tour Details" },
                { "Tour_NotFound", "Tour not found" },
                { "Tour_Guide", "Guide" },
                { "Tour_Locations", "Locations" },
                { "Tour_Dates", "Dates" },
                { "Tour_Duration", "Duration" },
                { "Tour_Price", "Price" },
                { "Tour_Rating", "Rating" },
                { "Tour_BookButton", "Book Tour" },
                { "Tour_LoginToBook", "Please login to book a tour" },
                { "TourType_Hiking", "Hiking" },
                { "TourType_Adventure", "Adventure" },
                { "TourType_Recovery", "Recovery" },
                { "Tours_Filter_Days", "days" },
                { "Tours_Filter_TourType", "Tour Type" },
                { "Messages_FailedToGetData", "Failed to get data" },
                { "Messages_TourNotFound", "Tour not found" },
                { "Messages_FailedToBookTour", "Failed to book tour" },
                { "Messages_TourBookedSuccessfully", "Tour booked successfully" }
            })),
            new Language(LocalizationCode.UKR, JsonSerializer.Serialize(new List<LanguageLocationsDto>
            {
                new(1, "Київ", "Україна"),
                new(2, "Львів", "Україна"),
                new(3, "Нью-Йорк", "Сполучені Штати"),
                new(4, "Лос-Анджелес", "Сполучені Штати"),
                new(5, "Лондон", "Велика Британія")
            }), JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Tour_Details_Title", "Деталі туру" },
                { "Tour_NotFound", "Тур не знайдено" },
                { "Tour_Guide", "Гід" },
                { "Tour_Locations", "Локації" },
                { "Tour_Dates", "Дати" },
                { "Tour_Duration", "Тривалість" },
                { "Tour_Price", "Ціна" },
                { "Tour_Rating", "Рейтинг" },
                { "Tour_BookButton", "Забронювати тур" },
                { "Tour_LoginToBook", "Будь ласка, увійдіть, щоб забронювати тур" },
                { "TourType_Hiking", "Похід" },
                { "TourType_Adventure", "Пригода" },
                { "TourType_Recovery", "Відпочинок" },
                { "Tours_Filter_Days", "днів" },
                { "Tours_Filter_TourType", "Тип туру" },
                { "Messages_FailedToGetData", "Не вдалося отримати дані" },
                { "Messages_TourNotFound", "Тур не знайдено" },
                { "Messages_FailedToBookTour", "Не вдалося забронювати тур" },
                { "Messages_TourBookedSuccessfully", "Тур успішно заброньовано" }
            }))
        };

        DbContext.Accounts.AddRange(accounts);
        DbContext.Guides.AddRange(guides);
        DbContext.Tours.AddRange(tours);
        DbContext.TourInstances.AddRange(tourInstances);
        DbContext.TourInstanceRates.AddRange(rates);
        DbContext.TourBookings.AddRange(bookings);
        DbContext.Languages.AddRange(languages);

        DbContext.SaveChanges();
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}