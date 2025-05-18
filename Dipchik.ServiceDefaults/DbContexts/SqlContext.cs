using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Model;
using Shared.Model.Dtos;
using System.Text.Json;

namespace Microsoft.Extensions.Hosting.DbContexts;

public class SqlContext : DbContext
{
    public SqlContext(DbContextOptions<SqlContext> options)
        : base(options)
    {
        Database.EnsureCreated();
        SeedTours();
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<AppEvent> Events { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourInstance> TourInstances { get; set; }
    public DbSet<TourInstanceRate> TourInstanceRates { get; set; }
    public DbSet<Guide> Guides { get; set; }
    public DbSet<TourBooking> TourBookings { get; set; }

    private void SeedTours()
    {
        if (!Locations.Any())
        {
            var data = Languages.Where(x => x.Locale == LocalizationCode.ENG).Select(x => x.CitiesAndCountriesJson).FirstOrDefault();
            var list = JsonSerializer.Deserialize<List<LanguageLocationsDto>>(data);
            Locations.AddRange(list.Select(x => new Location
            {
                GeoId = x.GeoId,
                Name = x.City,
                Country = x.Country
            }));
            SaveChanges();
        }

        // Create a test guide if none exists
        var guide = Guides.FirstOrDefault();
        if (guide == null)
        {
            // First create an account for the guide
            var guideAccount = Accounts.Where(x => x.Login == "cc").First();

            // Then create the guide
            guide = new Guide
            {
                Name = "John",
                Surname = "Smith",
                Age = 35,
                CareerStartUtc = DateTime.UtcNow.AddYears(-10),
                IsActive = true,
                AccountId = guideAccount.Id
            };
            Guides.Add(guide);
            SaveChanges();
        }

        if (!Tours.Any())
        {
            var tours = new List<Tour>
            {
                new Tour
                {
                    Title = "Majestic Alps Tour",
                    Description = "Explore the breathtaking beauty of the Swiss Alps. Experience stunning mountain views, charming alpine villages, and unforgettable hiking trails.",
                    Price = 2500,
                    DurationDays = 7,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Group,
                    TourType = TourTypeEnum.Hiking,
                    ImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 1, 2 }, // Geneva and Zurich
                    SpecialOffers = SpecialOfferEnum.None,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(10),
                            EndDate = DateTime.UtcNow.AddDays(17),
                            IsCancelled = false,
                            MaxParticipants = 20
                        }
                    }
                },
                new Tour
                {
                    Title = "Scenic Italy Journey",
                    Description = "Discover the rich history and culture of Italy. Visit iconic landmarks, enjoy authentic cuisine, and immerse yourself in the Italian way of life.",
                    Price = 3200,
                    DurationDays = 10,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Private,
                    TourType = TourTypeEnum.Sightseeing,
                    ImageUrl = "https://images.unsplash.com/photo-1464983953574-0892a716854b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 3, 4, 5 }, // Rome, Florence, Venice
                    SpecialOffers = SpecialOfferEnum.OnSale,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(20),
                            EndDate = DateTime.UtcNow.AddDays(30),
                            IsCancelled = false,
                            MaxParticipants = 15
                        }
                    }
                },
                new Tour
                {
                    Title = "Norwegian Fjords Adventure",
                    Description = "Sail through the stunning Norwegian fjords. Experience the dramatic landscapes, waterfalls, and charming coastal towns of Norway.",
                    Price = 4100,
                    DurationDays = 8,
                    WithGuide = false,
                    Classification = TourClassificationEnum.Private,
                    TourType = TourTypeEnum.Recreational,
                    ImageUrl = "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 6, 7 }, // Oslo and Bergen
                    SpecialOffers = SpecialOfferEnum.SpecialDiscount,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(40),
                            EndDate = DateTime.UtcNow.AddDays(48),
                            IsCancelled = false,
                            MaxParticipants = 30
                        }
                    }
                },
                new Tour
                {
                    Title = "Highlights of Belgium",
                    Description = "Experience the best of Belgium's culture, history, and cuisine. Visit medieval cities, taste world-famous chocolates, and explore historic landmarks.",
                    Price = 1800,
                    DurationDays = 5,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Group,
                    TourType = TourTypeEnum.Mixed,
                    ImageUrl = "https://images.unsplash.com/photo-1502082553048-f009c37129b9",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 8, 9, 10, 11 }, // Brussels, Bruges, Antwerp, Ghent
                    SpecialOffers = SpecialOfferEnum.StartsSoon,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(5),
                            EndDate = DateTime.UtcNow.AddDays(10),
                            IsCancelled = false,
                            MaxParticipants = 25
                        }
                    }
                },
                new Tour
                {
                    Title = "Carpathian Mountains Adventure",
                    Description = "Experience the beauty of Ukrainian Carpathians. Hike through pristine forests, visit traditional Hutsul villages, and enjoy local cuisine. Perfect for nature lovers and adventure seekers.",
                    Price = 1200,
                    DurationDays = 5,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Group,
                    TourType = TourTypeEnum.Hiking,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 12, 13 }, // Lviv and Ivano-Frankivsk
                    SpecialOffers = SpecialOfferEnum.None,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(15),
                            EndDate = DateTime.UtcNow.AddDays(20),
                            IsCancelled = false,
                            MaxParticipants = 15
                        }
                    }
                },
                new Tour
                {
                    Title = "Kyiv Cultural Heritage Tour",
                    Description = "Discover the rich history and culture of Ukraine's capital. Visit ancient cathedrals, explore the historic Podil district, and experience vibrant local life.",
                    Price = 800,
                    DurationDays = 4,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Group,
                    TourType = TourTypeEnum.Sightseeing,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 14 }, // Kyiv
                    SpecialOffers = SpecialOfferEnum.OnSale,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(25),
                            EndDate = DateTime.UtcNow.AddDays(29),
                            IsCancelled = false,
                            MaxParticipants = 20
                        }
                    }
                },
                new Tour
                {
                    Title = "Odesa Black Sea Retreat",
                    Description = "Enjoy the beautiful Black Sea coast in Odesa. Relax on sandy beaches, explore historic architecture, and experience the city's famous nightlife.",
                    Price = 1500,
                    DurationDays = 6,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Group,
                    TourType = TourTypeEnum.Recreational,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 15 }, // Odesa
                    SpecialOffers = SpecialOfferEnum.StartsSoon,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(35),
                            EndDate = DateTime.UtcNow.AddDays(41),
                            IsCancelled = false,
                            MaxParticipants = 18
                        }
                    }
                },
                new Tour
                {
                    Title = "Carpathian Recovery Retreat",
                    Description = "A specialized recovery program for Ukrainian soldiers in the peaceful Carpathian Mountains. Features therapeutic activities, psychological support, and nature-based healing.",
                    Price = 2000,
                    DurationDays = 10,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Private,
                    TourType = TourTypeEnum.Recovery,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 12, 13 }, // Lviv and Ivano-Frankivsk
                    SpecialOffers = SpecialOfferEnum.SpecialDiscount,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(30),
                            EndDate = DateTime.UtcNow.AddDays(40),
                            IsCancelled = false,
                            MaxParticipants = 8
                        }
                    }
                },
                new Tour
                {
                    Title = "Black Sea Rehabilitation Program",
                    Description = "A comprehensive rehabilitation program for soldiers at the Black Sea coast. Combines physical therapy, psychological support, and recreational activities in a peaceful environment.",
                    Price = 2500,
                    DurationDays = 14,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Private,
                    TourType = TourTypeEnum.Recovery,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 15 }, // Odesa
                    SpecialOffers = SpecialOfferEnum.SpecialDiscount,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(45),
                            EndDate = DateTime.UtcNow.AddDays(59),
                            IsCancelled = false,
                            MaxParticipants = 6
                        }
                    }
                },
                new Tour
                {
                    Title = "Kyiv Wellness & Recovery",
                    Description = "A specialized wellness program for soldiers in Kyiv. Features modern rehabilitation facilities, professional medical support, and cultural activities for holistic recovery.",
                    Price = 1800,
                    DurationDays = 7,
                    WithGuide = true,
                    Classification = TourClassificationEnum.Private,
                    TourType = TourTypeEnum.Recovery,
                    ImageUrl = "https://images.unsplash.com/photo-1598887141928-7c4c472b8b8b",
                    GuideId = guide.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Locations = new List<int> { 14 }, // Kyiv
                    SpecialOffers = SpecialOfferEnum.SpecialDiscount,
                    Instances = new List<TourInstance>
                    {
                        new TourInstance
                        {
                            StartDate = DateTime.UtcNow.AddDays(20),
                            EndDate = DateTime.UtcNow.AddDays(27),
                            IsCancelled = false,
                            MaxParticipants = 10
                        }
                    }
                }
            };

            Tours.AddRange(tours);
            SaveChanges();
        }

        if (!TourInstanceRates.Any())
        {
            var rates = new List<TourInstanceRate>
            {
                new TourInstanceRate
                {
                    TourInstanceId = 1,
                    Rate = 45, // 4.5 stars
                    TouristCommentary =
                        "Amazing experience in the Alps! The views were breathtaking and our guide was very knowledgeable.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-5),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 1,
                    Rate = 40, // 4.0 stars
                    TouristCommentary =
                        "Great tour, but the weather could have been better. Still enjoyed the experience.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-3),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 2,
                    Rate = 50, // 5.0 stars
                    TouristCommentary = "Perfect tour! The food, the culture, the history - everything was amazing!",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-2),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 2,
                    Rate = 48, // 4.8 stars
                    TouristCommentary = "Wonderful experience. The private tour made it even more special.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-1),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 3,
                    Rate = 42, // 4.2 stars
                    TouristCommentary = "Beautiful fjords and great accommodations. Would recommend!",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-4),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 4,
                    Rate = 38, // 3.8 stars
                    TouristCommentary = "Good tour overall, but some attractions were too crowded.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-6),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 4,
                    Rate = 45, // 4.5 stars
                    TouristCommentary = "Excellent chocolate tasting and historical sites!",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-7),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 5, // Carpathian Mountains Adventure
                    Rate = 48,
                    TouristCommentary = "Amazing experience in the Carpathians! The nature is breathtaking and the local culture is fascinating.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-10),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 6, // Kyiv Cultural Heritage Tour
                    Rate = 45,
                    TouristCommentary = "Great tour of Kyiv's historical sites. The guide was very knowledgeable about Ukrainian history.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-8),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 7, // Odesa Black Sea Retreat
                    Rate = 47,
                    TouristCommentary = "Perfect combination of relaxation and cultural experiences. The beaches are beautiful!",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-6),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 8, // Carpathian Recovery Retreat
                    Rate = 50,
                    TouristCommentary = "Life-changing experience. The program helped me recover both physically and mentally. The staff was incredibly supportive.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-15),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 9, // Black Sea Rehabilitation Program
                    Rate = 49,
                    TouristCommentary = "Excellent rehabilitation program. The combination of therapy and peaceful environment was perfect for recovery.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-12),
                    TouristAccountId = 1
                },
                new TourInstanceRate
                {
                    TourInstanceId = 10, // Kyiv Wellness & Recovery
                    Rate = 48,
                    TouristCommentary = "Professional and caring staff. The facilities are modern and the program is well-structured.",
                    RatedTimeUtc = DateTime.UtcNow.AddDays(-9),
                    TouristAccountId = 1
                }
            };
            
            TourInstanceRates.AddRange(rates);
            SaveChanges();
        }
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Account>().Property(x => x.Login).HasMaxLength(100);
        mb.Entity<Account>().Property(x => x.AccountId).HasMaxLength(50);
        mb.Entity<Account>().HasIndex(x => x.Login).IsUnique();
        mb.Entity<Account>().HasIndex(x => x.AccountId).IsUnique();
        mb.Entity<Account>().Property(x => x.Username).HasMaxLength(100);
        mb.Entity<Account>().HasQueryFilter(x => !x.IsDeleted);

        mb.Entity<Language>().Property(x => x.DisplayLocalizationsJson).HasColumnName("DisplayLocalizationsJson");

        base.OnModelCreating(mb);
    }
}