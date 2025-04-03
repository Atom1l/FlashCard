using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashCard.Data;
using FlashCard.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.Mime.MediaTypeNames;

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
                    // ✅ เก็บ UserID ไว้ใน Session
                    HttpContext.Session.SetInt32("UserID", user.UserID);

                    // ✅ เพิ่ม UserID ลงใน Claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // 🔥 จุดสำคัญ
                        new Claim(ClaimTypes.Name, user.UserName),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // ✅ ตั้งค่า Cookie Authentication
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: UserID from Claims = {userId}");
            }
            else
            {
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: UserID from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return RedirectToAction("Login");
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == "1" && i.SubModule == "1")
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                // Reset HasShown if User seen it all
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "1" && i.SubModule == "1"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                // No more Images left
                return View(null);
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            // Add data to UserCardDB to separate each users card showing
            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return View(selectedImage);
        }

        // ======================================== For Next FlashCard ======================================== //
        public async Task<IActionResult> GetNextFlashCard(string module, string submodule)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userNameClaim = User.Identity.Name;

            Console.WriteLine($"DEBUG: UserID (Claim) = {userIdClaim}, UserName (Claim) = {userNameClaim}");

            int userId = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: userId from Claims = {userId}");
            }
            else
            {
                // ถ้าไม่ได้จาก Claims, อ่านจาก Session
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: userId from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return BadRequest($"Invalid User ID: {userId}");
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == module && i.SubModule == submodule)
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                // ✅ รีเซ็ต HasShown เฉพาะของ userId นั้น ๆ
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == module && i.SubModule == submodule))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                return Json(new { success = false });
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                imgBytes = Convert.ToBase64String(selectedImage.Imgbytes),
                correctAnswer = selectedImage.Answer,
                subModule = selectedImage.SubModule
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
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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

            var random = new Random();
            var selectedImage = images[random.Next(images.Count)];

            if (selectedImage != null)
            {
                selectedImage.HasShown = true;
                _db.Entry(selectedImage).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);  // ส่งภาพที่เลือกไปยัง View
        }


        // Module 1 Start 2 //
        public IActionResult M1_Start2()
        {
            return View();
        }

        public async Task<IActionResult> M1_FlashCard2()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: UserID from Claims = {userId}");
            }
            else
            {
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: UserID from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return RedirectToAction("Login");
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == "1" && i.SubModule == "2")
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                // Reset HasShown if User seen it all
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "1" && i.SubModule == "2"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                // No more Images left
                return View(null);
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            // Add data to UserCardDB to separate each users card showing
            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return View(selectedImage);
        }


        // Module 1 Enhance Understanding2 //
        public IActionResult M1_Enhance2()
        {
            return View();
        }

        public async Task<IActionResult> M1_Enhance2_Test()
        {
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);  // ส่งภาพที่เลือกไปยัง View
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

        // FlashCard Details //
        public IActionResult FlashCard_Details(string name, string module)
        {
            ViewData["Module"] = module;
            var selectedImg = _db.ImagesDB.FirstOrDefault(i => i.Name == name);
            if (selectedImg == null)
            {
                return RedirectToAction("M1_Conclude_All");
            }
            return View(selectedImg);
        }

        // Module 1 Test //
        public IActionResult M1_Test_Start()
        {
            return View();
        }

        // Module 1 Test // // ศำหรับต้นไผ่ก็อปวางตั้งแค่ M1_Test เลย แล้วก็เปลี่ยนเลยเป็น 3 ทั้งชื่อมันและเลข Module ข้างใน //
        public async Task<IActionResult> M1_Test()
        {
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var images = await _db.ImagesDB
                .Where(i => (i.Module == "1") && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "1")
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

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);
        }

        public async Task<IActionResult> GetNextFlashCard_Test(string module)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userNameClaim = User.Identity.Name;

            Console.WriteLine($"DEBUG: UserID (Claim) = {userIdClaim}, UserName (Claim) = {userNameClaim}");

            int userId = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: userId from Claims = {userId}");
            }
            else
            {
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: userId from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return BadRequest($"Invalid User ID: {userId}");
            }

            // Count from UserId and Img that has already set in database //
            int shownCount = await _db.UserCardDB
                .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == module))
                .CountAsync();

            // ====================== TO DELETE HASSHOWN AFTER DONE ====================== //
            if (module == "1" && shownCount >= 12)
            {
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "1"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                return Json(new { success = false });
            }
            else if (module == "2" && shownCount >= 8)
            {
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "2"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                return Json(new { success = false });
            }
            else if (module == "3" && shownCount >= 7)
            {
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "3"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                return Json(new { success = false });
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == module)
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                return Json(new { success = false }); 
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                imgBytes = Convert.ToBase64String(selectedImage.Imgbytes),
                correctAnswer = selectedImage.Answer,
                subModule = selectedImage.SubModule
            });
        }



        // Module 1 Test done //
        public async Task<IActionResult> M1_Test_Done(string score, string module)
        {
            ViewData["Score"] = score;
            ViewData["Module"] = module;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                // ลบข้อมูล UserCardDB ที่เกี่ยวข้องกับผู้ใช้
                var userCards = _db.UserCardDB.Where(uf => uf.UserId == userId && uf.Image.Module == module);
                _db.UserCardDB.RemoveRange(userCards);
                await _db.SaveChangesAsync();

                // เช็คว่าโมดูลคืออะไรและบันทึกคะแนนไปยังคอลัมน์ที่เหมาะสม
                var user = await _db.UsersDB.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user != null)
                {
                    switch (module)
                    {
                        case "M1":
                            user.M1Score = int.TryParse(score, out int m1Score) ? m1Score : 0;
                            break;
                        case "M2":
                            user.M2Score = int.TryParse(score, out int m2Score) ? m2Score : 0;
                            break;
                        case "M3":
                            user.M3Score = int.TryParse(score, out int m3Score) ? m3Score : 0;
                            break;
                        default:
                            // ถ้าโมดูลไม่ตรงกับที่กำหนด
                            break;
                    }

                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                return BadRequest("Invalid user or module.");
            }

            return View();
        }

        /* ================================== Module 2 ================================== */

        // === MODULE 2 === //
        // Module 2 Main Page //
        public IActionResult M2_Main()
        {
            return View();
        }

        // Module 2 ASK //
        public IActionResult M2_Ask()
        {
            return View();
        }

        // Module 2 Tutorial //
        public IActionResult M2_Tutorial()
        {
            return View();
        }

        // Module 2 Choose //
        public IActionResult M2_Choose()
        {
            return View();
        }

        // Module 2 Start 1 //
        public IActionResult M2_Start1()
        {
            return View();
        }

        // Module 2 Start 2 
        public IActionResult M2_Start2()
        {
            return View();
        }

        // Module 2 Enhance 1
        public IActionResult M2_Enhance1()
        {
            return View();
        }
        public async Task<IActionResult> M2_Enhance1_Test()
        {
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var images = await _db.ImagesDB
                .Where(i => i.Module == "2" && i.SubModule == "1" && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "2" && i.SubModule == "1")
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

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);  // ส่งภาพที่เลือกไปยัง View
        }

        public async Task<IActionResult> M2_Enhance2_Test()
        {
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var images = await _db.ImagesDB
                .Where(i => i.Module == "2" && i.SubModule == "2" && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "2" && i.SubModule == "2")
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

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);  // ส่งภาพที่เลือกไปยัง View
        }

        // Module 2 Enhance 2
        public IActionResult M2_Enhance2()
        {
            return View();
        }

        // Module 2 Conclude
        public IActionResult M2_Conclude()
        {
            return View();
        }

        public async Task<IActionResult> M2_Conclude_All()
        {
            var conclude_img = await _db.ImagesDB.Where(i => i.Module == "2").ToListAsync();
            return View(conclude_img);
        }

        // Module 1 Test //
        public IActionResult M2_Test_Start()
        {
            return View();
        }

        // Module 2 Done
        public IActionResult M2_Done(string source, string next)
        {
            ViewData["Source"] = source;
            ViewData["Next"] = next;
            return View();
        }


        // Module 2 FlashCard
        public async Task<IActionResult> M2_FlashCard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: UserID from Claims = {userId}");
            }
            else
            {
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: UserID from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return RedirectToAction("Login");
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == "2" && i.SubModule == "1")
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                // Reset HasShown if User seen it all
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "2" && i.SubModule == "1"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                // No more Images left
                return View(null);
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            // Add data to UserCardDB to separate each users card showing
            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return View(selectedImage);
        }

        // Module 2 FlashCard2
        public async Task<IActionResult> M2_FlashCard2()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId))
            {
                Console.WriteLine($"DEBUG: UserID from Claims = {userId}");
            }
            else
            {
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                Console.WriteLine($"DEBUG: UserID from Session = {userId}");
            }

            if (userId <= 0 || !await _db.UsersDB.AnyAsync(u => u.UserID == userId))
            {
                return RedirectToAction("Login");
            }

            var unseenImages = await _db.ImagesDB
                .Where(i => i.Module == "2" && i.SubModule == "2")
                .Where(i => !_db.UserCardDB.Any(uf => uf.UserId == userId && uf.ImageId == i.Id))
                .ToListAsync();

            if (!unseenImages.Any())
            {
                // Reset HasShown if User seen it all
                await _db.UserCardDB
                    .Where(uf => uf.UserId == userId && _db.ImagesDB.Any(i => i.Id == uf.ImageId && i.Module == "2" && i.SubModule == "2"))
                    .ExecuteDeleteAsync();

                await _db.SaveChangesAsync();

                // No more Images left
                return View(null);
            }

            var selectedImage = unseenImages[new Random().Next(unseenImages.Count)];

            // Add data to UserCardDB to separate each users card showing
            _db.UserCardDB.Add(new UserFlashCard
            {
                UserId = userId,
                ImageId = selectedImage.Id,
                HasShown = true
            });

            await _db.SaveChangesAsync();

            return View(selectedImage);
        }

        // Module 2 Test
        public async Task<IActionResult> M2_Test()
        {
            // ดึง User ID จาก session หรือ claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var images = await _db.ImagesDB
                .Where(i => (i.Module == "2") && (i.HasShown == false || i.HasShown == null))
                .ToListAsync();

            if (!images.Any())
            {
                await _db.ImagesDB
                    .Where(i => i.Module == "2")
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

                // บันทึกการแสดงภาพใน UserCardDB
                _db.UserCardDB.Add(new UserFlashCard
                {
                    UserId = userId,
                    ImageId = selectedImage.Id,
                    HasShown = true
                });

                await _db.SaveChangesAsync();
            }

            return View(selectedImage);
        }

    }


}
