using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebsite1.Data;
using MyWebsite1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DownloadsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DownloadsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /*// list of downloads
    public IActionResult Index()
    {
        var downloads = _context.Downloads.ToList(); // Get all downloads
        return View(downloads);
    }*/


    // added paging
    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        var totalDownloads = _context.Downloads.Count();
        var totalPages = (int)Math.Ceiling(totalDownloads / (double)pageSize);

        var downloads = _context.Downloads
            .OrderByDescending(d => d.DateCreated) // новите най-отгоре
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPageNumber = page;
        ViewBag.TotalPages = totalPages;

        return View(downloads);
    }

    // download upload form
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Handle download upload
    [HttpPost]
    public async Task<IActionResult> Create(Download download, IFormFile uploadedFile)
    {
        if (string.IsNullOrEmpty(download.Title))
        {
            TempData["Error"] = "Please provide a title.";
            return View(download);
        }

        if (uploadedFile == null || uploadedFile.Length <= 0)
        {
            TempData["Error"] = "Please provide a file.";
            return View(download);
        }

        // Converts the uploaded file to a byte array
        using (var memoryStream = new System.IO.MemoryStream())
        {
            await uploadedFile.CopyToAsync(memoryStream);
            download.FileData = memoryStream.ToArray();
            download.FileType = uploadedFile.ContentType; // Sets the file type (e.g., application/pdf, image/jpeg)
        }

        download.UserId = _userManager.GetUserId(User);
        download.DateCreated = DateTime.Now;

        _context.Downloads.Add(download);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // Handle the file deletion
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var download = await _context.Downloads.FindAsync(id);

        if (download == null || download.UserId != _userManager.GetUserId(User))
        {
            return Unauthorized(); // Ensure the current user is the uploader
        }

        _context.Downloads.Remove(download);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); // Redirect back to the downloads list
    }

    // Show details of a single download
    public IActionResult Details(int id)
    {
        var download = _context.Downloads.FirstOrDefault(d => d.Id == id);

        if (download == null)
        {
            return NotFound();
        }

        return View(download);
    }

    // Download file action
    public IActionResult Download(int id)
    {
        var download = _context.Downloads.Find(id);
        if (download == null) return NotFound();

        var contentType = string.IsNullOrEmpty(download.FileType) ? "application/octet-stream" : download.FileType;

        // Get file extension from MIME type
        string extension = MimeTypes.GetExtension(contentType);

        // Ensure title has proper extension
        var fileName = download.Title;
        if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            fileName += extension;
        }

        return File(download.FileData, contentType, fileName);
    }
}

// Helper class to map MIME types to file extensions
public static class MimeTypes
{
    private static readonly Dictionary<string, string> MimeToExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"image/jpeg", ".jpg"},
        {"image/png", ".png"},
        {"image/gif", ".gif"},
        {"application/pdf", ".pdf"},
        {"application/zip", ".zip"},
        {"text/plain", ".txt"},
        // add more MIME types and extensions as needed
    };

    public static string GetExtension(string mimeType)
    {
        if (MimeToExtension.TryGetValue(mimeType, out var ext))
        {
            return ext;
        }
        return ""; // default no extension if unknown
    }
}
