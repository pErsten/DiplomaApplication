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
