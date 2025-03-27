using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashCard.Data;
using FlashCard.Models;

namespace FlashCard.Controllers
{
    
    public class FlashCardController : Controller
    {
        // Start Page //
        public IActionResult Home()
        {
            return View();
        }

        // Login Page //
        public IActionResult Login()
        {
            return View();
        }

        // Register Page //
        public IActionResult Register()
        {
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
        public IActionResult M1_Done(string source)
        {
            TempData["Source"] = source; // เก็บค่าไว้ข้ามหน้า
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
