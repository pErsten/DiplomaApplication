using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;

namespace Dipchik.Services;


public record class GeoNamesCityDto(
    int GeoId,
    string Name,
    float Latitude,
    float Longitude,
    GeoFeatureCode CityCode,
    string Country);

public record class GeoNamesLocalizationsDto(
    int geonameid,
    LocalizationCode locale,
    string Name,
    bool isPreferredName,
    bool isShortName,
    bool isColloquial,
    bool isHistoric);

public class GeoNamesCountryDto
{
    public int GeoId { get; set; }
    public string CountryCode { get; set; }
    public Dictionary<LocalizationCode, string> Names { get; set; } = new Dictionary<LocalizationCode, string>();
}

// TODO: move to Shared
public record class LanguageLocationsDto(int GeoId, string City, string Country);

public class CityLocation
{
    public int GeoId { get; set; }
    public float Latitude { get; set; }
    public float Longitude {get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public Dictionary<LocalizationCode, (string, string)> Names { get; set; } = new Dictionary<LocalizationCode, (string, string)>();
}
public class LocationsParser
{
    private readonly SqlContext dbContext;

    public LocationsParser(SqlContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task UpdateLocalizations(CancellationToken stoppingToken = new CancellationToken())
    {
        var data = await ParseCountries(stoppingToken);
        var dic = new Dictionary<LocalizationCode, List<LanguageLocationsDto>>();
        

        foreach (var item in data)
        {
            foreach (var lang in item.Names)
            {
                if (!dic.TryGetValue(lang.Key, out var list))
                {
                    list = [];
                    dic[lang.Key] = list;
                }
                list.Add(new LanguageLocationsDto(item.GeoId, lang.Value.Item1, lang.Value.Item2));
            }
        }

        //dbContext.Languages.Add(new Language{ Locale = LocalizationCode.ENG, CitiesAndCountriesJson = "", Json = "" });
        //dbContext.Languages.Add(new Language{ Locale = LocalizationCode.UKR, CitiesAndCountriesJson = "", Json = "" });
        //dbContext.Languages.Add(new Language{ Locale = LocalizationCode.RUS, CitiesAndCountriesJson = "", Json = "" });
        //await dbContext.SaveChangesAsync(stoppingToken);
        var dbLocalizations = await dbContext.Languages.ToListAsync(stoppingToken);
        foreach (var lang in dbLocalizations)
        {
            var json = JsonSerializer.Serialize(dic[lang.Locale]);
            lang.CitiesAndCountriesJson = json;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    public async Task<List<CityLocation>> ParseCountries(CancellationToken stoppingToken)
    {
        var dic = new Dictionary<int, CityLocation>();

        var fileLocation = Path.Combine(Environment.CurrentDirectory, "countryInfo.txt");
        StreamReader countriesReader = new StreamReader(fileLocation);
        var line = await countriesReader.ReadLineAsync(stoppingToken);
        var countriesDic = new Dictionary<int, GeoNamesCountryDto>();
        while (!countriesReader.EndOfStream)
        {
            var splitArr = line.Split('\t');
            var geoId = int.Parse(splitArr[16]);
            countriesDic[geoId] = new GeoNamesCountryDto
            {
                GeoId =  geoId,
                CountryCode = splitArr[0]
            };
            line = await countriesReader.ReadLineAsync(stoppingToken);
        }

        fileLocation = Path.Combine(Environment.CurrentDirectory, "cities500.txt");
        StreamReader citiesReader = new StreamReader(fileLocation);
        line = await citiesReader.ReadLineAsync(stoppingToken);
        while (!citiesReader.EndOfStream)
        {
            var splitArr = line.Split('\t');
            var cityCode = EnumSwitches.GetGeoFeatureCode(splitArr[7]);

            if(cityCode != GeoFeatureCode.None)
            {
                dic[int.Parse(splitArr[0])] = new CityLocation
                {
                    GeoId = int.Parse(splitArr[0]),
                    Latitude = float.Parse(splitArr[4], CultureInfo.InvariantCulture),
                    Longitude = float.Parse(splitArr[5], CultureInfo.InvariantCulture),
                    Name = splitArr[1],
                    CountryCode = splitArr[8]
                };
            }

            line = await citiesReader.ReadLineAsync(stoppingToken);
        }

        citiesReader.Close();

        fileLocation = Path.Combine(Environment.CurrentDirectory, "alternateNames.txt");
        StreamReader localizationsReader = new StreamReader(fileLocation);
        line = await localizationsReader.ReadLineAsync(stoppingToken);
        while (!localizationsReader.EndOfStream)
        {
            var splitArr = line.Split('\t');
            var info = new GeoNamesLocalizationsDto(
                geonameid: int.Parse(splitArr[1]),
                locale: EnumSwitches.GetLocalizationCode(splitArr[2]),
                Name: splitArr[3],
                isPreferredName: splitArr[4] == "1",
                isShortName: splitArr[5] == "1",
                isColloquial: splitArr[6] == "1",
                isHistoric: splitArr[7] == "1"
            );

            line = await localizationsReader.ReadLineAsync(stoppingToken);

            if (info.isShortName || info.isColloquial || info.isHistoric || info.locale == LocalizationCode.None)
            {
                continue;
            }

            if (countriesDic.TryGetValue(info.geonameid, out var countryLocalization))
            {
                if (!countryLocalization.Names.ContainsKey(info.locale) || info.isPreferredName)
                {
                    countryLocalization.Names[info.locale] = info.Name;
                }
            }
            if (dic.TryGetValue(info.geonameid, out var cityLocalization))
            {
                if (!cityLocalization.Names.ContainsKey(info.locale) || info.isPreferredName)
                {
                    cityLocalization.Names[info.locale] = (info.Name, null);
                }
            }
        }

        var newCountriesDic = countriesDic.Values.ToDictionary(x => x.CountryCode);
        var res = dic.Values.Where(x => x.Names.Count >= 3).ToList();

        foreach (var item in res)
        {
            if (newCountriesDic.TryGetValue(item.CountryCode, out var countryLocalization))
            {
                foreach (var lang in item.Names)
                {
                    item.Names[lang.Key] = (lang.Value.Item1, countryLocalization.Names[lang.Key]);
                }
            }
        }

        return res;
    }
}