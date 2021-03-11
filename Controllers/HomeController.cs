using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Data;
using TestingSystem.Models;
using TestingSystem.ViewModels;

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

        public IActionResult StartTesting()
        {
            List<Question> questions = GetRandomQuestions();
            return View("Testing", questions);
        }
        [HttpPost]
        public IActionResult FinishTesting([FromBody] FinishedViewModel vm)
        {
            var student = db.Students
                            .Where(s => s.FirstName == vm.FirstName && s.LastName == vm.LastName)
                            .FirstOrDefault();
            if (student is null)
            {
                student = new Student() { FirstName = vm.FirstName, LastName = vm.LastName };
                db.Students.Add(student);
                db.SaveChanges();
            }
            CheckAnswers(vm);
            vm.StudentAnswers.ForEach(sa =>
            {
                sa.StudentId = student.Id;
                sa.Date = DateTime.Now;
            });
            db.StudentAnswers.AddRange(vm.StudentAnswers);
            db.SaveChanges();
            return Json(new { Link = Url.ActionLink("Index", "Home") });
        }
        private List<Question> GetRandomQuestions()
        {
            List<Question> questions = new List<Question>();
            Random rnd = new Random();
            int max = db.Questions.Count();
            int skip = rnd.Next(1, max);
            while (questions.Count < 3)
            {
                var row = db.Questions.Skip(skip).Take(1).First();
                if (row.ques_type == ques_type.POL && !questions.Contains(row))
                    questions.Add(row);
                skip = rnd.Next(1, max);
            }
            while (questions.Count < 6)
            {
                var row = db.Questions.Skip(skip).Take(1).First();
                if (row.ques_type == ques_type.CHL && !questions.Contains(row))
                    questions.Add(row);
                skip = rnd.Next(1, max);
            }
            while (questions.Count < 8)
            {
                var row = db.Questions.Skip(skip).Take(1).First();
                if (row.ques_type == ques_type.UPR && !questions.Contains(row))
                    questions.Add(row);
                skip = rnd.Next(1, max);
            }
            return questions;
        }

        private void CheckAnswers(FinishedViewModel vm)
        {
            string studAns = "";
            string ans = "";
            ques_type ques_Type;
            for (int i = 0; i < vm.StudentAnswers.Count; i++)
            {
                studAns = vm.StudentAnswers[i].Answer.ToLower();
                ans = db.Questions.Find(vm.StudentAnswers[i].QuestionId).Answer.ToLower();
                ques_Type = db.Questions.Find(vm.StudentAnswers[i].QuestionId).ques_type;
                if (studAns == ans)
                {
                    switch (ques_Type)
                    {
                        case ques_type.POL:
                            vm.StudentAnswers[i].Point = 1;
                            break;
                        case ques_type.CHL:
                            vm.StudentAnswers[i].Point = 2;
                            break;
                        case ques_type.UPR:
                            vm.StudentAnswers[i].Point = 4;
                            break;
                    }
                }
            }
        }
    }
}
