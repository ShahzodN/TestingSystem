using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TestingSystem.Data;
using TestingSystem.Models;
using TestingSystem.ViewModels;

namespace TestingSystem.Controllers
{
    public class DiagramController : Controller
    {
        private readonly DataContext db;
        private List<Course> Courses { get; set; }

        public DiagramController(DataContext _db)
        {
            db = _db;
            Courses = db.Courses.ToList();
        }

        public IActionResult Index()
        {
            return View(Courses);
        }

        public IActionResult Create()
        {
            return View(Courses);
        }

        public async Task<IActionResult> GetRandomData(string firstName, string lastName, int courseId)
        {
            Random rnd = new Random();
            var course = await db.Courses.FindAsync(courseId);
            if (course is null)
                return NotFound(new { Message = "Предмет не найден"});

            double[][] matrix = new double[course.ChaptersCount + 1][];

            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new double[4];
                if (i == matrix.Length - 1)
                {
                    matrix[i][0] = Math.Pow(matrix[0][0] * matrix[1][0] * matrix[2][0], 1d / 3);
                    matrix[i][1] = Math.Pow(matrix[0][1] * matrix[1][1] * matrix[2][1], 1d / 3);
                    matrix[i][2] = Math.Pow(matrix[0][2] * matrix[1][2] * matrix[2][2], 1d / 3);
                    matrix[i][3] = Math.Pow(matrix[0][0] * matrix[1][3] * matrix[2][3], 1d / 3);
                }
                else
                {
                    for (int j = 0; j < matrix[i].Length; j++)
                    {
                        if (j == 3)
                            matrix[i][j] = Math.Pow(matrix[i][0] * matrix[i][1] * matrix[i][2], 1d / 3);
                        else
                            matrix[i][j] = rnd.NextDouble();
                    }
                }
            }
            return Json(new { ChaptersCount = course.ChaptersCount, Matrix = matrix });
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
                return Json(new { data = new double[] { UPR / 8, CHL / 6, POL / 3 }, Quaility = Math.Pow(POL / 3 * CHL / 6 * UPR / 8, 1d / 3) });
            }
            else
                return NotFound();
        }
    }
}
