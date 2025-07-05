using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.Models;

namespace UMT88.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        /* ==================== LOGIN ==================== */

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email và mật khẩu không được để trống.");
                return View();
            }

            var user = await _db.Users
                                .FirstOrDefaultAsync(u => u.email == email && u.password_hash == password);
            if (user == null)
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu.");
                return View();
            }

            // Lưu session để dùng phân quyền sau này
            HttpContext.Session.SetInt32("UserId", (int)user.user_id);
            HttpContext.Session.SetInt32("RoleId", user.role_id);

            // Redirect theo role
            if (user.role_id == 1)
            {
                // Admin
                return RedirectToAction("Index", "AdminDashboard");
            }
            else
            {
                // Bettor (người chơi) → User Dashboard
                return RedirectToAction("Index", "UserDashboard");
            }
        }


        /* ==================== REGISTER ==================== */

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name)
             || string.IsNullOrWhiteSpace(email)
             || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
                return View();
            }

            // Mật khẩu phải ít nhất 6 ký tự
            if (password.Length < 6)
            {
                ModelState.AddModelError("password", "Mật khẩu phải có ít nhất 6 ký tự.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận không khớp.");
                return View();
            }

            if (await _db.Users.AnyAsync(u => u.email == email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                return View();
            }

            var user = new User
            {
                name = name,
                email = email,
                password_hash = password,  // TODO: hash trước khi lưu
                balance = 0,
                status = "active",
                role_id = 2,         // Mặc định Bettor
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        /* ============ FORGOT PASSWORD (FAKE) ============ */

        // GET: /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email.");
                return View();
            }

            // kiểm tra email tồn tại
            if (!_db.Users.Any(u => u.email == email))
            {
                ModelState.AddModelError("", "Email không tìm thấy trong hệ thống.");
                return View();
            }

            // sinh mã 4 số và lưu tạm
            var code = new Random().Next(1000, 9999).ToString();
            TempData["ResetEmail"] = email;
            TempData["FakeCode"] = code;

            // log ra console để debug
            Console.WriteLine($"[FAKE EMAIL] Mã reset cho {email} là {code}");

            return RedirectToAction("VerifyCode");
        }


        /* ================= VERIFY CODE ================= */

        // GET: /Auth/VerifyCode
        [HttpGet]
        public IActionResult VerifyCode()
        {
            var email = TempData.Peek("ResetEmail") as string;
            if (email == null) return RedirectToAction("ForgotPassword");

            ViewBag.Email = email;
            ViewBag.FakeCode = TempData.Peek("FakeCode") as string;
            return View();
        }

        // POST: /Auth/VerifyCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyCode(string email, string code)
        {
            var realCode = TempData.Peek("FakeCode") as string;
            if (string.IsNullOrWhiteSpace(code) || realCode == null || code != realCode)
            {
                ModelState.AddModelError("", "Mã không hợp lệ hoặc đã hết hạn.");
                ViewBag.Email = email;
                ViewBag.FakeCode = realCode;
                return View();
            }

            TempData["Verified"] = true;
            return RedirectToAction("ResetPassword");
        }


        /* ================ RESET PASSWORD ================ */

        // GET: /Auth/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData.Peek("ResetEmail") as string;
            var verified = TempData.Peek("Verified") as bool?;
            if (email == null || verified != true)
                return RedirectToAction("ForgotPassword");

            ViewBag.Email = email;
            return View();
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp hoặc để trống.");
                ViewBag.Email = email;
                return View();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại.");
                ViewBag.Email = email;
                return View();
            }

            user.password_hash = newPassword;  // TODO: hash lại trước khi lưu
            user.updated_at = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        /* =================== LOGOUT =================== */

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Xoá session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
