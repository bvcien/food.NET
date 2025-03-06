using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCORE.Models;
using NETCORE.Data;

namespace NETCORE.Controllers;

public class HoaquaController : Controller
{
    private const string CartSessionKey = "Cart";
    private readonly ApplicationDbContext _context;

    public HoaquaController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _context = context;

    }
    public IActionResult Hoaqua()
    {
        var hoaquas = _context.Hoaqua.ToList();
        return View(hoaquas);  // Trả dữ liệu vào View
    }
    public IActionResult Pay()
    {

        return View();  // Trả dữ liệu vào View
    }
    public async Task<IActionResult> Chitiet(int? id)
    {
        // Kiểm tra nếu id là null
        if (id == null)
        {
            return NotFound(); // Trả về 404 nếu id không hợp lệ
        }

        // Truy vấn cơ sở dữ liệu bất đồng bộ với FirstOrDefaultAsync
        var hoaqua = await _context.Hoaqua.FirstOrDefaultAsync(m => m.Id == id);

        // Kiểm tra nếu không tìm thấy sản phẩm
        if (hoaqua == null)
        {
            return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
        }
        return View(hoaqua);
    }

    public IActionResult AddToCart(int id, int quantity)
    {
        var product = _context.Hoaqua.FirstOrDefault(p => p.Id == id);

        if (product != null)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingProduct = cart.FirstOrDefault(item => item.Id == id);

            if (existingProduct != null)
            {
                // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
                existingProduct.Quantity += quantity;
            }
            else
            {
                // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
                cart.Add(new CartItem
                {
                    Id = product.Id,
                    HoaquaId = product.Id,
                    Hoaqua = product,
                    Title = product.Title ?? "Chưa có tên", // Nếu Title null, gán giá trị mặc định
                    Genre = product.Genre ?? "Chưa có thể loại", // Nếu Genre null, gán giá trị mặc định
                    Price = product.Price > 0 ? product.Price : 0, // Nếu Price null hoặc < 0, gán giá trị mặc định
                    ImageUrl = product.ImageUrl ?? "default_image.jpg", // Nếu ImageUrl null, gán ảnh mặc định
                    Quantity = quantity
                });
            }

            // Lưu giỏ hàng vào Session
            HttpContext.Session.SetObject("Cart", cart);
        }

        return RedirectToAction("Giohang", "Hoaqua"); // Chuyển hướng về trang chủ hoặc giỏ hàng
    }
    [HttpPost]
    public IActionResult UpdateCart(int id, int quantity)
    {
        // Kiểm tra xem số lượng có hợp lệ không
        if (quantity < 1)
        {
            return Json(new { success = false, message = "Số lượng không hợp lệ." });
        }

        var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

        // Tìm sản phẩm trong giỏ hàng
        var existingProduct = cart.FirstOrDefault(item => item.Id == id);

        if (existingProduct != null)
        {
            // Cập nhật số lượng sản phẩm
            existingProduct.Quantity = quantity;

            // Lưu lại giỏ hàng đã cập nhật vào session
            HttpContext.Session.SetObject("Cart", cart);
        }

        return Json(new { success = true });
    }
    public IActionResult GioHang()
    {
        // Lấy giỏ hàng từ Session
        var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart); // Trả về view hiển thị giỏ hàng
    }


}

