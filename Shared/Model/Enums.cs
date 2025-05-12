namespace Shared.Model;

[Flags]
public enum AccountRolesEnum
{
    None = 0,
    Client = 1 << 0,
    Modify = 1 << 1,
    Guide = 1 << 2
}

public enum GeoFeatureCode
{
    None = 0,
    PPL = 1,
    PPLC = 2,
    PPLL = 3,
}

public enum LocalizationCode
{
    None = 0,
    UKR = 1,
    ENG = 2,
    RUS = 3
}

public enum EventTypeEnum
{
    None = 0,
    LocationsLocalizationsUpdated = 1,
    DisplayLocalizationsUpdated = 2,
}

public enum TourTypeEnum
{
    None = 0,
    Recreational = 1,
    Hiking = 2,
    Mixed = 3,
    Recovery = 4,
    Sightseeing = 5,
}

[Flags]
public enum SpecialOfferEnum
{
    None = 0,
    OnSale = 1 << 0,
    StartsSoon = 1 << 1,
    SpecialDiscount = 1 << 2
}

public enum DestinationCountEnum
{
    None = 0,
    Single = 1,
    TwoToThree = 2,
    FourOrMore = 3
}

public enum TourInstanceStatus
{
    None = 0,
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3
}
public enum TourClassificationEnum
{
    Private,
    Group
}

public static class EnumSwitches
{
    public static LocalizationCode GetLocalizationCode(string code) => code switch
    {
        "uk" => LocalizationCode.UKR,
        "en" => LocalizationCode.ENG,
        "ru" => LocalizationCode.RUS,
        _ => LocalizationCode.None
    };

    public static GeoFeatureCode GetGeoFeatureCode(string code) => code switch
    {
        "PPLC" => GeoFeatureCode.PPLC,
        "PPLL" => GeoFeatureCode.PPLL,
        "PPL" => GeoFeatureCode.PPL,
        _ => GeoFeatureCode.None
    };
}
