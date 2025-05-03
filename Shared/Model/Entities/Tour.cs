using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Tour
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
}