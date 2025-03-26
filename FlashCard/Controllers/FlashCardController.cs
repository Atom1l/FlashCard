using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashCard.Data;
using FlashCard.Models;

namespace FlashCard.Controllers
{
    public class FlashCardController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
