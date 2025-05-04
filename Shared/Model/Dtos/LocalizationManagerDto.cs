namespace Shared.Model.Dtos;

public class LocalizationManagerDto
{
    public string Placeholder { get; set; } = string.Empty;
    public Dictionary<string, string> Translations { get; set; } = new(); // language -> value
}