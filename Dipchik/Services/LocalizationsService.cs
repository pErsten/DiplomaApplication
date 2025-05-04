using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;

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


public class LocalizationsService
{
    private readonly SqlContext dbContext;
    private readonly ChannelWriter<EventDto> eventProceeder;

    public LocalizationsService(SqlContext dbContext, ChannelWriter<EventDto> eventProceeder)
    {
        this.dbContext = dbContext;
        this.eventProceeder = eventProceeder;
    }

    public record class LanguageDisplayDto(LocalizationCode locale, string placeholder, string value);

    public async Task<Dictionary<LocalizationCode, Dictionary<string, string>>> GetAllDisplayLocalizations(CancellationToken stoppingToken)
    {
        var data = await dbContext.Languages.Select(x => new KeyValuePair<LocalizationCode, string>(x.Locale, x.DisplayLocalizationsJson)).ToListAsync(stoppingToken);
        var result = data.ToDictionary(x => x.Key,
            x => JsonSerializer.Deserialize<Dictionary<string, string>>(x.Value));
        
        return result;
    }

    public async Task UpdateDisplayLocalizations(Dictionary<LocalizationCode, Dictionary<string, string>> data, CancellationToken stoppingToken)
    {
        var codes = data.Keys.ToList();
        var dbLanguages = await dbContext.Languages.Where(x => codes.Contains(x.Locale)).ToListAsync(stoppingToken);
        foreach (var lang in dbLanguages)
        {
            var json = JsonSerializer.Serialize(data[lang.Locale]);
            lang.DisplayLocalizationsJson = json;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        await eventProceeder.WriteAsync(new EventDto(EventTypeEnum.DisplayLocalizationsUpdated, DateTime.UtcNow, data), stoppingToken);
    }
    public async Task UpdateLocaleLocalizations(CancellationToken stoppingToken)
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

        
        var dbLocalizations = await dbContext.Languages.ToListAsync(stoppingToken);
        if (dic.Any(x => !dbLocalizations.Exists(y => y.Locale == x.Key)))
        {
            var existingLocales = dbLocalizations.Select(x => x.Locale);
            var nonExistantLangs = dic.Keys.Except(existingLocales);
            foreach (var lang in nonExistantLangs)
            {
                var init = new List<KeyValuePair<string, string>>
                {
                    new("init", "_")
                };
                dbContext.Languages.Add(new Language(locale: lang, citiesAndCountriesJson: "[]", displayLocalizationsJson: JsonSerializer.Serialize(init)));
            }
            await dbContext.SaveChangesAsync(stoppingToken);

            dbLocalizations = await dbContext.Languages.ToListAsync(stoppingToken);
        }
        foreach (var lang in dbLocalizations)
        {
            var json = JsonSerializer.Serialize(dic[lang.Locale]);
            lang.CitiesAndCountriesJson = json;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        await eventProceeder.WriteAsync(new EventDto(EventTypeEnum.LocationsLocalizationsUpdated, DateTime.UtcNow, data), stoppingToken);
    }

    public async Task<List<CityLocationDto>> ParseCountries(CancellationToken stoppingToken)
    {
        var dic = new Dictionary<int, CityLocationDto>();

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
                dic[int.Parse(splitArr[0])] = new CityLocationDto
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