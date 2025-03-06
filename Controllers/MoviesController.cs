using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NETCORE.Models;
using NETCORE.Data;

namespace NETCORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(
       string sortOrder,
       string currentFilter,
       string searchString,
       int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var hoaquas = from s in _context.Hoaqua
                          select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                hoaquas = hoaquas.Where(s => s.Title.Contains(searchString)
                                       || s.Genre.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    hoaquas = hoaquas.OrderByDescending(s => s.Title);
                    break;
                case "Date":
                    hoaquas = hoaquas.OrderBy(s => s.Genre);
                    break;
                case "date_desc":
                    hoaquas = hoaquas.OrderByDescending(s => s.Genre);
                    break;
                default:
                    hoaquas = hoaquas.OrderBy(s => s.Title);
                    break;
            }
            int pageSize = 5; // Số lượng sản phẩm trên mỗi trang




            // Trả về danh sách được phân trang
            return View(await PaginatedList<Hoaqua>.CreateAsync(hoaquas, pageNumber ?? 1, pageSize));
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaqua = await _context.Hoaqua
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hoaqua == null)
            {
                return NotFound();
            }

            return View(hoaqua);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Price,ImageUrl")] Hoaqua hoaqua, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return View(hoaqua);
            }

            try
            {
                // Kiểm tra nếu có file ảnh được tải lên
                if (image != null && image.Length > 0)
                {
                    // Đường dẫn thư mục lưu trữ ảnh
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Lưu file ảnh vào thư mục đích
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn tương đối vào database
                    hoaqua.ImageUrl = Path.Combine("img", fileName);
                }

                // Thêm sản phẩm vào database
                _context.Add(hoaqua);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần thiết
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }

            return View(hoaqua);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaqua = await _context.Hoaqua.FindAsync(id);
            if (hoaqua == null)
            {
                return NotFound();
            }
            return View(hoaqua);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Price,ImageUrl")] Hoaqua hoaqua, IFormFile ImageFile)
        {
            if (id != hoaqua.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra nếu không có file
                    if (ImageFile == null || ImageFile.Length == 0)
                    {
                        ModelState.AddModelError("ImageFile", "Please upload an image.");
                    }

                    // Nếu có file, xử lý upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Tạo thư mục nếu chưa có
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh vào model
                        hoaqua.ImageUrl = "img/" + fileName;
                    }

                    _context.Update(hoaqua);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoaquaExists(hoaqua.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Chuyển lỗi sang ViewData nếu có lỗi từ ModelState
            ViewData["ImageFileError"] = ModelState["ImageFile"]?.Errors.FirstOrDefault()?.ErrorMessage;

            return View(hoaqua);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaqua = await _context.Hoaqua
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hoaqua == null)
            {
                return NotFound();
            }

            return View(hoaqua);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaqua = await _context.Hoaqua.FindAsync(id);
            if (hoaqua != null)
            {
                _context.Hoaqua.Remove(hoaqua);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HoaquaExists(int id)
        {
            return _context.Hoaqua.Any(e => e.Id == id);
        }
    }
}
