using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Data;
using TestingSystem.Models;

namespace TestingSystem.Controllers
{
    public class DiagramController : Controller
    {
        private readonly DataContext db;

        public DiagramController(DataContext _db)
        {
            db = _db;
        }
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> GetData(string firstName, string lastName)
        {
            var student = db.Students
                            .Where(s => s.FirstName == firstName && s.LastName == lastName)
                            .FirstOrDefaultAsync();
            if (student.Result is not null)
            {
                double POL, UPR, CHL;
                var studentAnswers = await db.StudentAnswers.Where(sa => sa.StudentId == student.Result.Id).Include(sa => sa.Question).OrderByDescending(sa => sa.Date).Take(8).ToListAsync();
                POL = studentAnswers.Where(sa => sa.Question.ques_type == ques_type.POL).Sum(sa => sa.Point);
                UPR = studentAnswers.Where(sa => sa.Question.ques_type == ques_type.UPR).Sum(sa => sa.Point);
                CHL = studentAnswers.Where(sa => sa.Question.ques_type == ques_type.CHL).Sum(sa => sa.Point);
                return Json(new { data = new double[] { POL / 3, CHL / 6, UPR / 8 }, Quaility = Math.Pow(POL / 3 * CHL / 6 * UPR / 8, 1d / 3) });
            }
            else
                return NotFound();
        }
    }
}
