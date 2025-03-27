using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashCard.Data;
using FlashCard.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace FlashCard.Controllers
{
    
    public class FlashCardController : Controller
    {
        // Database Refer //
        private readonly FlashCardDBContext _db;

        public FlashCardController(FlashCardDBContext db)
        {
            _db = db;
        }

        // Start Page //
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult HomeLoggedIn()
        {
            return View();
        }

        // Login Page //
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var user = _db.UsersDB.SingleOrDefault(u => u.UserName == username && u.Password == password);
                if (user != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                // เพิ่ม claims อื่น ๆ ตามที่ต้องการ
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // ตั้งค่า cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("HomeLoggedIn");
                }
                else
                {
                    return RedirectToAction("Home");
                }
            }
            return View();
        }

        // Log Out //
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Home");
        }


        // Register Page //
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(string name, string surname, string username, string password)
        {
            if (ModelState.IsValid) {
                _db.UsersDB.Add(new User
                {
                    Name = name,
                    Surname = surname,
                    UserName = username,
                    Password = password,
                    Streak = 0,
                    M1Score = 0, M2Score = 0, M3Score = 0
                });
                _db.SaveChanges();
            }
            return RedirectToAction("Home");
        }

        // Menu Page //
        public IActionResult Menu(string source)
        {
            TempData["Source"] = source;
            return View();
        }

        // Main Menu Page //
        public IActionResult Mainmenu()
        {
            return View();
        }

        // === MODULE 1 === //
        // Module 1 Main Page //
        public IActionResult M1_Main()
        {
            return View();
        }

        // Module 1 ASK //
        public IActionResult M1_Ask()
        {
            return View();
        }

        // Module 1 Tutorial //
        public IActionResult M1_Tutorial()
        {
            return View();
        }

        // Module 1 Start 1 //
        public IActionResult M1_Start1()
        {
            return View();
        }

        // Module 1 FlashCard(Emotion) //
        

        // Module 1 Done //
        public IActionResult M1_Done(string source, string next)
        {
            ViewData["Source"] = source;
            ViewData["Next"] = next; // รอแฟลชการ์ดก่อนแล้วจะใส่ asp-route-next = "Viewsต่อไป" เข้าไปเพื่อที่จะไปต่อยังหน้าถัดไปโดยไม่ต้องสร้างเยอะแยะ //
            return View();
        }

        // Module 1 Enhance Understanding //
        public IActionResult M1_Enhance1()
        {
            return View();
        }

        // Module 1 Start 2 //
        public IActionResult M1_Start2()
        {
            return View();
        }

        // Module 1 Enhance Understanding2 //
        public IActionResult M1_Enhance2()
        {
            return View();
        }

        // Module 1 Conclude //
        public IActionResult M1_Conclude()
        {
            return View();
        }

        // Module 1 Test //
        public IActionResult M1_Test_Start()
        {
            return View();
        }

        // Module 1 Test //


        // Module 1 Test done //
        public IActionResult M1_Test_Done()
        {
            return View();
        }
    }


}
