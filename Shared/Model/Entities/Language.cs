using Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Language
{
    [Key]
    public LocalizationCode Locale { get; set; }
    public string CitiesAndCountriesJson { get; set; }
    public string Json { get; set; }
}