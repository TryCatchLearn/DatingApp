using System.ComponentModel.DataAnnotations.Schema;
using API.Entities;

namespace API;

[Table("Photos")]
public class Photo
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }
    public bool IsApproved { get; set; }
    public string PublicId { get; set; }
    public int AppUserId { get; set; }
    public AppUser appUser { get; set; }
}
