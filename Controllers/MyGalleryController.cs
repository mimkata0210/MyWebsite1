using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebsite1.Data;
using MyWebsite1.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class MyGalleryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyGalleryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Show the user's gallery (only their photos)
    public IActionResult Index()
    {
        var userId = _userManager.GetUserId(User);
        var photos = _context.Photos.Where(p => p.UserId == userId).ToList();

        return View(photos);
    }

    // Show the photo upload form
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Handle photo upload
    [HttpPost]
    public async Task<IActionResult> Create(Photo photo, IFormFile uploadedFile)
    {
        if (string.IsNullOrEmpty(photo.Title))
        {
            TempData["Error"] = "Please provide a title.";
            return View(photo);
        }

        if (uploadedFile == null || uploadedFile.Length == 0)
        {
            TempData["Error"] = "Please provide an image.";
            return View(photo);
        }

        if (!uploadedFile.ContentType.StartsWith("image/"))
        {
            TempData["Error"] = "Only image files are allowed.";
            return RedirectToAction("Create");
        }

        using (var memoryStream = new MemoryStream())
        {
            await uploadedFile.CopyToAsync(memoryStream);
            photo.ImageData = memoryStream.ToArray();
        }

        photo.UserId = _userManager.GetUserId(User);
        photo.DateCreated = DateTime.Now;

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // Handle photo deletion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null || photo.UserId != _userManager.GetUserId(User))
        {
            return Unauthorized();
        }

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return Json(new { success = true, id = id });
    }

}
