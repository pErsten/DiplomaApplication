using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model.Dtos;
using Shared.Model;

namespace Backend.Services
{
    public class BookingService
    {
        private readonly SqlContext dbContext;
        private ILogger<BookingService> logger;

        public BookingService(ILoggerFactory loggerFactory, SqlContext dbContext)
        {
            logger = loggerFactory.CreateLogger<BookingService>();
            this.dbContext = dbContext;
        }

        public async Task<OperationResult> BookTour(Account account, int id, CancellationToken stoppingToken)
        {
            var tour = await dbContext.TourInstances
                .Where(x => x.Id == id)
                .Select(x => new { x.IsCancelled, x.MaxParticipants, x.Tour.Price, x.EndDate })
                .FirstOrDefaultAsync(stoppingToken);
            if (tour is null)
            {
                logger.LogError("Tour with ID {id} not found", id);
                return new OperationResult(ErrorCodes.ErrorCode_TourNotFound);
            }
            if (TourInstance.GetStatus(tour.IsCancelled, tour.EndDate) != TourInstanceStatus.Scheduled)
            {
                logger.LogError("Tour with ID {id} is not available for booking", id);
                return new OperationResult(ErrorCodes.ErrorCode_TourIsNotAvailableForBooking);
            }

            var bookings = await dbContext.TourBookings
                .Where(x => x.TourInstanceId == id && x.AccountId == account.Id && x.CancellationDate == null)
                .Select(x => new
                {
                    x.Id,
                    x.AccountId
                })
                .ToListAsync(stoppingToken);
            if (bookings.Count >= tour.MaxParticipants)
            {
                logger.LogError("Tour with ID {id} is fully booked", id);
                return new OperationResult(ErrorCodes.ErrorCode_TourIsFullyBooked);
            }
            if (bookings.Any(x => x.AccountId == account.Id))
            {
                logger.LogError("Tour with ID {id} is already booked by account {accountId}", id, account.Id);
                return new OperationResult(ErrorCodes.ErrorCode_TourIsBookedByAccount);
            }

            var booking = new TourBooking
            {
                TourInstanceId = id,
                AccountId = account.Id,
                BookedUtc = DateTime.UtcNow,
                TotalPrice = tour.Price
            };

            await dbContext.TourBookings.AddAsync(booking, stoppingToken);
            await dbContext.SaveChangesAsync(stoppingToken);

            return OperationResult.Success();
        }

        public async Task<OperationResult<List<BookingDto>>> GetMyBookings(Account account, CancellationToken stoppingToken)
        {
            var bookings = await dbContext.TourBookings
                .Where(x => x.AccountId == account.Id)
                .Include(x => x.TourInstance)
                .ThenInclude(x => x.Tour)
                .Include(x => x.TourInstance)
                .ThenInclude(x => x.Rates)
                .OrderByDescending(x => x.BookedUtc)
                .Select(x => new BookingDto
                {
                    Id = x.Id,
                    TourInstanceId = x.TourInstanceId,
                    TourTitle = x.TourInstance.Tour.Title,
                    StartDate = x.TourInstance.StartDate,
                    EndDate = x.TourInstance.EndDate,
                    TotalPrice = x.TotalPrice,
                    IsCancelled = x.IsCancelled,
                    HasRated = x.TourInstance.Rates.Any(r => r.TouristAccountId == account.Id)
                })
                .ToListAsync(stoppingToken);

            return new OperationResult<List<BookingDto>>(bookings);
        }

        public async Task<OperationResult> CancelBooking(Account account, int id, CancellationToken stoppingToken)
        {
            var booking = await dbContext.TourBookings
                .Include(x => x.TourInstance)
                .FirstOrDefaultAsync(x => x.Id == id && x.AccountId == account.Id, stoppingToken);

            if (booking is null)
            {
                logger.LogError("Booking with ID {id} not found for account {accountId}", id, account.Id);
                return new OperationResult(ErrorCodes.ErrorCode_BookingNotFound);
            }

            if (booking.IsCancelled)
            {
                logger.LogError("Booking with ID {id} is already cancelled", id);
                return new OperationResult(ErrorCodes.ErrorCode_BookingIsCancelled);
            }

            if (booking.TourInstance.StartDate <= DateTime.UtcNow.AddDays(1))
            {
                logger.LogError("Booking with ID {id} is too late to be cancelled for account {account}", id, account.Id);
                return new OperationResult(ErrorCodes.ErrorCode_TooLateToCancelBooking);
            }

            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Cancelled by user";

            await dbContext.SaveChangesAsync(stoppingToken);

            return OperationResult.Success();
        }
    }
}
