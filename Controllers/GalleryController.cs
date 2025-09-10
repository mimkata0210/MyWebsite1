using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebsite1.Data;
using MyWebsite1.Models;
using System.Security.Claims;

namespace MyWebsite1.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Added paging - its either that or lazy loading
        public async Task<IActionResult> Feed(int page = 1, int pageSize = 12)
        {
            var totalItems = await _context.Photos.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await _context.Photos
                .OrderByDescending(g => g.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPageNumber = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PagingAction = "Feed";
            ViewBag.PagingController = "Gallery";

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var photo = await _context.Photos
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
                return NotFound();

            var comments = await _context.Comments
                .Where(c => c.PhotoId == id)
                .Include(c => c.User)
                .OrderBy(c => c.DateCreated)
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return View(photo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int photoId, string content)
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
            await _context.SaveChangesAsync();

            // Зареждам User от базата, за да взема UserName
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return Json(new
            {
                success = true,
                comment = new
                {
                    Id = comment.Id, // to dos
                    User = comment.User?.UserName ?? "Unknown",
                    userId = comment.UserId,
                    Content = comment.Content,
                    DateCreated = comment.DateCreated.ToLocalTime().ToString("g")
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return Json(new { success = false, message = "Comment not found." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || comment.UserId != userId)
                return Json(new { success = false, message = "Unauthorized." });

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}