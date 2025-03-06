
using System.ComponentModel.DataAnnotations;

namespace NETCORE.Models;
public class CartItem
{
    public int Id { get; set; }
    public int HoaquaId { get; set; }  // Khóa ngoại tới Hoaqua
    public required Hoaqua Hoaqua { get; set; }
    public string? Title { get; set; }
    
    public string? Genre { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }  // Số lượng sản phẩm trong giỏ hàng
}
