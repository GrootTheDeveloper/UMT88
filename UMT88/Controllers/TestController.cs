using Microsoft.AspNetCore.Mvc;
using UMT88.Data;

public class TestController : Controller
{
    private readonly AppDbContext _db;
    public TestController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        var users = _db.Users.Take(5).ToList();
        return Json(users);
    }
}
