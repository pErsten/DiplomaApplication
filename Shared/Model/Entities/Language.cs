using Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Language
{
    [Key]
    public LocalizationCode Locale { get; set; }
    public string CitiesAndCountriesJson { get; set; }
    public string DisplayLocalizationsJson { get; set; }

    public Language(LocalizationCode locale, string citiesAndCountriesJson, string displayLocalizationsJson)
    {
        Locale = locale;
        CitiesAndCountriesJson = citiesAndCountriesJson;
        DisplayLocalizationsJson = displayLocalizationsJson;
    }
}