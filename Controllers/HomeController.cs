using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Data;
using TestingSystem.Models;

namespace TestingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext db;

        public HomeController(DataContext _db)
        {
            db = _db;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
