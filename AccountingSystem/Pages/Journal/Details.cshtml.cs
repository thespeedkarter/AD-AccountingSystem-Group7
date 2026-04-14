using AccountingSystem.Data;
using AccountingSystem.Models;
using AccountingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Journal
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPostingService _posting;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DetailsModel(
            ApplicationDbContext db,
            IPostingService posting,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env)
        {
            _db = db;
            _posting = posting;
            _userManager = userManager;
            _env = env;
        }

        public JournalEntry Entry { get; set; } = default!;

        public bool CanPost { get; private set; }

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entry = await _db.JournalEntries
                .AsNoTracking()
                .Include(e => e.Lines)
                    .ThenInclude(l => l.ChartAccount)
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.JournalEntryId == id);

            if (entry == null) return NotFound();

            Entry = entry;

            var totalD = Entry.Lines.Sum(l => l.Debit);
            var totalC = Entry.Lines.Sum(l => l.Credit);
            var isBalanced = totalD == totalC && totalD > 0m;

            CanPost =
                User.IsInRole("Manager") &&
                Entry.Status == JournalStatus.Approved &&
                isBalanced;

            return Page();
        }

        public async Task<IActionResult> OnPostPostAsync(int id)
        {
            if (!User.IsInRole("Manager"))
                return Forbid();

            var userId = _userManager.GetUserId(User);
            var (ok, message) = await _posting.PostAsync(id, userId);

            if (ok) StatusMessage = message;
            else ErrorMessage = message;

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostUploadAsync(int id)
        {
            if (!(User.IsInRole("Manager") || User.IsInRole("Accountant")))
                return Forbid();

            if (UploadFile == null || UploadFile.Length == 0)
            {
                ErrorMessage = "Please choose a file to upload.";
                return RedirectToPage("./Details", new { id });
            }

            var allowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".jpg", ".jpeg", ".png" };

            var ext = Path.GetExtension(UploadFile.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !allowedExts.Contains(ext))
            {
                ErrorMessage = "Invalid file type. Allowed: pdf, word, excel, csv, jpg, png.";
                return RedirectToPage("./Details", new { id });
            }

            var entry = await _db.JournalEntries.FirstOrDefaultAsync(e => e.JournalEntryId == id);
            if (entry == null) return NotFound();

            var folderRel = Path.Combine("uploads", "journal", id.ToString());
            var folderAbs = Path.Combine(_env.WebRootPath, folderRel);
            Directory.CreateDirectory(folderAbs);

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(folderAbs, storedName);

            await using (var stream = new FileStream(absPath, FileMode.Create))
            {
                await UploadFile.CopyToAsync(stream);
            }

            var userId = _userManager.GetUserId(User);

            _db.JournalAttachments.Add(new JournalAttachment
            {
                JournalEntryId = id,
                OriginalFileName = UploadFile.FileName,
                StoredFileName = storedName,
                ContentType = UploadFile.ContentType ?? "application/octet-stream",
                SizeBytes = UploadFile.Length,
                UploadedAtUtc = DateTime.UtcNow,
                UploadedByUserId = userId
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Attachment uploaded.";
            return RedirectToPage("./Details", new { id });
        }
    }
}