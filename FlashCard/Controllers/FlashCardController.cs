﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashCard.Data;
using FlashCard.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        // Upload Image to database //
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string answer, string module, string subModule)
        {
            if (file != null && file.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var imageModel = new Images
                {
                    Name = file.FileName,
                    Module = module,
                    SubModule = subModule,
                    Answer = answer,
                    Imgbytes = memoryStream.ToArray(),
                    HasShown = false
                };

                _db.ImagesDB.Add(imageModel);
                await _db.SaveChangesAsync();

                ViewBag.Message = "Upload Successful!";
            }
            else
            {
                ViewBag.Message = "Please select an image!";
            }

            return View();
        }

        // Show Pic //
        public async Task<IActionResult> ShowAllImages()
        {
            var images = await _db.ImagesDB.ToListAsync();
            return View(images);
        }


        // =============================================================================== //
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

        // ======================================== Module 1 FlashCard(Emotion) ======================================== //
        public async Task<IActionResult> M1_FlashCard()
        {
            var images = await _db.ImagesDB
                .Where(i => i.Module == "1" && i.SubModule == "1" && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "1" && i.SubModule == "1")
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.HasShown, false));

                await _db.SaveChangesAsync();
            }

            var selectedImage = images[new Random().Next(images.Count)];
            selectedImage.HasShown = true;
            _db.Entry(selectedImage).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return View(selectedImage);
        }

        // ======================================== For Next FlashCard ======================================== //
        public async Task<IActionResult> GetNextFlashCard(string module , string submodule)
        {
            var images = await _db.ImagesDB
                .Where(i => i.Module == module && i.SubModule == submodule && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == module && i.SubModule == submodule)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.HasShown, false));

                await _db.SaveChangesAsync();
                return Json(new { success = false });
            }

            var selectedImage = images[new Random().Next(images.Count)];
            selectedImage.HasShown = true;
            _db.Entry(selectedImage).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                imgBytes = Convert.ToBase64String(selectedImage.Imgbytes),
                correctAnswer = selectedImage.Answer
            });
        }



        // Module 1 Done //
        public IActionResult M1_Done(string source, string next)
        {
            ViewData["Source"] = source;
            ViewData["Next"] = next;
            return View();
        }

        // Module 1 Enhance Understanding //
        public IActionResult M1_Enhance1()
        {
            return View();
        }

        // Module 1 Enhance Test //
        public async Task<IActionResult> M1_Enhance1_Test()
        {
            var images = await _db.ImagesDB
            .Where(i => i.Module == "1" && i.SubModule == "2" && (i.HasShown == false || i.HasShown == null))
            .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "1" && i.SubModule == "2")
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.HasShown, false));

                await _db.SaveChangesAsync();
            }

            var random = new Random();
            var selectedImage = images[random.Next(images.Count)];

            if (selectedImage != null)
            {
                selectedImage.HasShown = true;
                _db.Entry(selectedImage).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }

            return View(selectedImage);
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

        public async Task<IActionResult> M1_Conclude_All()
        {
            var conclude_img = await _db.ImagesDB.Where(i => i.Module == "1").ToListAsync();
            return View(conclude_img);
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
