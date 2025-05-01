using System.ComponentModel.DataAnnotations;
using Shared.Model;
using Shared.Model.Dtos;

namespace Common.Model.Entities;

public class AppEvent
{
    [Key]
    public int Id { get; set; }
    public DateTime UtcCreated { get; set; }
    public EventTypeEnum EventType { get; set; }
    public string EventJsonData { get; set; }

    public AppEvent() { }

    public AppEvent(EventDto dto, string eventJsonData)
    {
        UtcCreated = dto.UtcCreated;
        EventType = dto.EventType;
        EventJsonData = eventJsonData;
    }
}