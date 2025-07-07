// Controllers/WithdrawController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;       // cho AppDbContext
using UMT88.Models;     // cho Transaction, User, WithdrawRequest
using UMT88.ViewModels; // cho WithdrawRowVm

namespace UMT88.Controllers
{
    public class WithdrawController : Controller
    {
        private readonly AppDbContext _db;
        public WithdrawController(AppDbContext db) => _db = db;

        /* --- Danh sách --- */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rows = await _db.Withdraw_Requests
                .Include(w => w.user)
                .OrderByDescending(w => w.requested_at)
                .Select(w => new WithdrawRowVm(
                    w.withdraw_id,
                    w.user.name,
                    w.amount,
                    w.bank_account,
                    w.requested_at,
                    w.status))
                .ToListAsync();

            return View(rows);
        }

        /* --- Duyệt --- */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            var req = await _db.Withdraw_Requests
                               .Include(w => w.user)
                               .FirstOrDefaultAsync(w => w.withdraw_id == id);
            if (req == null || req.status != "pending")
            {
                TempData["Toast"] = "Yêu cầu không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            // Trừ xu khỏi user
            req.user.balance -= req.amount;
            req.status = "approved";
            req.processed_at = DateTime.UtcNow;

            // Ghi vào bảng Transactions
            _db.Transactions.Add(new Transaction
            {
                user_id = req.user_id,
                transaction_type = "withdraw",
                amount = -req.amount,
                balance_after = req.user.balance,
                created_at = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Đã duyệt rút tiền";
            return RedirectToAction(nameof(Index));
        }

        /* --- Huỷ --- */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id)
        {
            var req = await _db.Withdraw_Requests
                               .FirstOrDefaultAsync(w => w.withdraw_id == id);
            if (req == null || req.status != "pending")
            {
                TempData["Toast"] = "Yêu cầu không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            req.status = "rejected";
            req.processed_at = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Toast"] = "Đã huỷ yêu cầu";
            return RedirectToAction(nameof(Index));
        }
    }
}
