using System.ComponentModel.DataAnnotations;

namespace NETCORE.Models;

public class Hoaqua
{
    public int Id { get; set; }
    public string? Title { get; set; }
    [DataType(DataType.Date)]

    public string? Genre { get; set; }
    [DisplayFormat(DataFormatString = "{0:N0} VND")]
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    // public ICollection<CartItem> CartItem { get; set; }

}