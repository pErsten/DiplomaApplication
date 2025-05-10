using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Model.Entities;

public class Guide
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
    public byte Age { get; set; }
    public DateTime CareerStartUtc { get; set; }
    public bool IsActive { get; set; }

    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account Account { get; set; }
}