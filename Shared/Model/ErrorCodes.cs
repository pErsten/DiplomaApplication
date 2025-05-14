namespace Shared.Model;

public class ErrorCodes
{
    // General
    public const string ErrorCode_PageMustBeBiggerThanZero = "ErrorCode_PageMustBeBiggerThanZero";
    public const string ErrorCode_PageSizeNotAllowed = "ErrorCode_PageSizeNotAllowed";

    // Account
    public const string ErrorCode_Unauthorized = "ErrorCode_Unauthorized";
    public const string ErrorCode_AccountNotFound = "ErrorCode_AccountNotFound";
    public const string ErrorCode_TriedToRegisterExistingAccount = "ErrorCode_TriedToRegisterExistingAccount";
    public const string ErrorCode_AccountWrongPassword = "ErrorCode_AccountWrongPassword";

    // Tours
    public const string ErrorCode_TourNotFound = "ErrorCode_TourNotFound";
    public const string ErrorCode_TourIsNotAvailableForBooking = "ErrorCode_TourIsNotAvailableForBooking";
    public const string ErrorCode_TourIsFullyBooked = "ErrorCode_TourIsFullyBooked";
    public const string ErrorCode_TourIsBookedByAccount = "ErrorCode_TourIsBookedByAccount";
    public const string ErrorCode_CannotRateRatedTour = "ErrorCode_CannotRateRatedTour";
    public const string ErrorCode_CannotRateNotCompletedTour = "ErrorCode_CannotRateNotCompletedTour";
    public const string ErrorCode_CannotRateNotBookedTour = "ErrorCode_CannotRateNotBookedTour";

    // Bookings
    public const string ErrorCode_BookingNotFound = "ErrorCode_BookingNotFound";
    public const string ErrorCode_BookingIsCancelled = "ErrorCode_BookingIsCancelled";
    public const string ErrorCode_TooLateToCancelBooking = "ErrorCode_TooLateToCancelBooking";

    // Localization
    public const string ErrorCode_LocaleNotFound = "ErrorCode_LocaleNotFound";
    public const string ErrorCode_LocaleNotSupported = "ErrorCode_LocaleNotSupported";

    // AI
    public const string ErrorCode_FailedToGetOpenApiResponse = "ErrorCode_FailedToGetOpenApiResponse";
    public const string ErrorCode_InvalidOpenApiResponse = "ErrorCode_InvalidOpenApiResponse";
    public const string ErrorCode_FailedToParseOpenApiResponse = "ErrorCode_FailedToParseOpenApiResponse";
}