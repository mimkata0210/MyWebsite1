using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebsite1.Data;
using MyWebsite1.Models;
using System;
using System.Linq;
using System.Security.Claims;

public class GalleryController : Controller
{
    private readonly ApplicationDbContext _context;

    public GalleryController(ApplicationDbContext context)
    {
        _context = context;
    }

    /* делете латер - old v
     * public IActionResult Feed()
    {
        ViewData["CurrentPage"] = "Browse Feed";
        var photos = _context.Photos.Include(p => p.User).OrderByDescending(p => p.DateCreated).ToList(); // вика от базата чрез _context.Photos, Include(p=>p.User) - зарежда свързания потребител,
                                                                                                          // извивка ToList() - изпълнява заявката и връща резултатите като списък.
        return View(photos);
    }
    */

    // Added paging - its either that or lazy loading
    public IActionResult Feed(int page = 1, int pageSize = 10)
    {
        ViewData["CurrentPage"] = "Browse Feed";

        var photos = _context.Photos
            .Include(p => p.User)
            .OrderByDescending(p => p.DateCreated)
            .Skip((page -1) * pageSize) // скип превиоус лоадс
            .Take(pageSize)
            .ToList();

        // Number of records to calculate the entire page
        var totalPhotos = _context.Photos.Count();
        var totalPages = (int)Math.Ceiling(totalPhotos / (double)pageSize);

        ViewBag.CurrentPageNumber = page;
        ViewBag.TotalPages = totalPages;

        return View(photos);
    }

    public IActionResult Details(int id)
    {
        var photo = _context.Photos.Include(p => p.User).FirstOrDefault(p => p.Id == id);
        if (photo == null)
            return NotFound();

        var comments = _context.Comments
            .Where(c => c.PhotoId == id)
            .Include(c => c.User)
            .OrderBy(c => c.DateCreated)
            .ToList();

        ViewBag.Comments = comments;
        ViewBag.CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return View(photo);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult AddComment(int photoId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Json(new { success = false, message = "Comment cannot be empty." });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var comment = new Comment
        {
            PhotoId = photoId,
            UserId = userId,
            Content = content,
            DateCreated = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        _context.SaveChanges();

        // Зареждам User от базата, за да взема UserName
        _context.Entry(comment).Reference(c => c.User).Load();

        // Връщам дата в локално време и правилно форматирана
        return Json(new
        {
            success = true,
            comment = new
            {
                Id = comment.Id,        // to dos
                User = comment.User?.UserName ?? "Unknown",
                userId = comment.UserId,
                Content = comment.Content,
                DateCreated = comment.DateCreated.ToLocalTime().ToString("g")  // Формат "dd.MM.yyyy HH:mm"
            }
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteComment(int id)
    {
        var comment = _context.Comments.FirstOrDefault(c => c.Id == id);
        if (comment == null)
            return Json(new { success = false, message = "Comment not found." });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || comment.UserId != userId)
            return Json(new { success = false, message = "Unauthorized." });

        _context.Comments.Remove(comment);
        _context.SaveChanges();

        return Json(new { success = true });
    }
}
