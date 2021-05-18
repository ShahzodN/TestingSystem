using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		private List<Course> Courses { get; set; }

		public HomeController(DataContext _db)
		{
			db = _db;
			Courses = db.Courses.ToList();
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task Add()
		{
			string a = "VC";
			int j = 115;
			for (int i = 0; i < 20; i++)
			{
				if (i < 5 && i >= 0)
				{
					db.Questions.Add(new Question() { Difficulty = 2, Answer = j.ToString(), ChapterId = 4, ques_type = ques_type.CHL, Text = a + j });
				}
				else if (i >= 5 && i <= 9)
				{
					db.Questions.Add(new Question() { Difficulty = 2, Answer = j.ToString(), ChapterId = 5, ques_type = ques_type.CHL, Text = a + j });
				}
				else if (i > 9 && i < 15)
				{
					db.Questions.Add(new Question() { Difficulty = 2, Answer = j.ToString(), ChapterId = 6, ques_type = ques_type.CHL, Text = a + j });
				}
				else
				{
					db.Questions.Add(new Question() { Difficulty = 2, Answer = j.ToString(), ChapterId = 7, ques_type = ques_type.CHL, Text = a + j });

				}
				j++;
			}
			await db.SaveChangesAsync();
		}

		public async Task Remove()
		{
			var ques = await db.Questions.Where(q => (q.ChapterId == 4 || q.ChapterId == 5 || q.ChapterId == 6 || q.ChapterId == 7) && (q.ques_type == ques_type.CHL)).ToListAsync();
			db.Questions.RemoveRange(ques);
			await db.SaveChangesAsync();
		}

		public IActionResult SelectCourse()
		{
			return View(Courses);
		}

		public IActionResult Image()
		{
			return View();
		}

		public IActionResult SelectChapter(int courseId)
		{
			var chapters = db.Chapters.Where(c => c.CourseId == courseId).ToList();
			return View(chapters);
		}

		public async Task<IActionResult> StartTesting(int chapterId, int courseId = -1)
		{
			List<Question> questions;
			if (courseId != -1)
				questions = await GetCourseQuestions(courseId);
			else
				questions = await GetChapterQuestions(chapterId);
			if (questions is null)
				throw new NullReferenceException();
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
				student = new Student() { FirstName = vm.FirstName, LastName = vm.LastName, Group = db.Groups.Where(g => g.Number == vm.Number).FirstOrDefault() };
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

		private async Task<List<Question>> GetCourseQuestions(int courseId)
		{
			List<Question> questions = new List<Question>();
			var course = await db.Courses.Include(c => c.Chapters).Where(c => c.Id == courseId).FirstOrDefaultAsync();
			var chapters = course.Chapters.ToList();
			List<Question> questionsFromDatabase = await db.Questions
														   .Include(q => q.Chapter)
														   .ThenInclude(c => c.Course)
														   .Where(q => q.Chapter.Course.Id == courseId).ToListAsync();
			if (questionsFromDatabase.Count == 0)
				return null;
			Random rnd = new Random();
			Question rndQuestion;
			int max = questionsFromDatabase.Count;
			int skip = rnd.Next(0, max);
			while (questions.Count < course.ChaptersCount * 3)
			{
				for (int i = 0; i < course.ChaptersCount;)
				{
					rndQuestion = questionsFromDatabase.Skip(skip).Take(1).First();
					if (rndQuestion.ques_type == ques_type.POL && !questions.Contains(rndQuestion) && rndQuestion.ChapterId == chapters[i].Id)
					{
						questions.Add(rndQuestion);
						if (questions.Count % 3 == 0)
							i++;
					}
					skip = rnd.Next(0, max);
				}
			}
			while (questions.Count < (course.ChaptersCount * 3) + (course.ChaptersCount * 3))
			{
				for (int i = 0; i < course.ChaptersCount;)
				{
					rndQuestion = questionsFromDatabase.Skip(skip).Take(1).First();
					if (rndQuestion.ques_type == ques_type.CHL && !questions.Contains(rndQuestion) && rndQuestion.ChapterId == chapters[i].Id)
					{
						questions.Add(rndQuestion);
						if (questions.Count % 3 == 0)
							i++;
					}
					skip = rnd.Next(0, max);
				}

			}
			while (questions.Count < (course.ChaptersCount * 2) + (course.ChaptersCount * 3) + (course.ChaptersCount * 3))
			{
				for (int i = 0; i < course.ChaptersCount;)
				{
					rndQuestion = questionsFromDatabase.Skip(skip).Take(1).First();
					if (rndQuestion.ques_type == ques_type.UPR && !questions.Contains(rndQuestion) && rndQuestion.ChapterId == chapters[i].Id)
					{
						questions.Add(rndQuestion);
						if (questions.Count % 2 == 0)
							i++;
					}
					skip = rnd.Next(0, max);
				}
			}
			return questions;
		}

		private async Task<List<Question>> GetChapterQuestions(int chapterId)
		{
			List<Question> questions = new List<Question>();
			var ques = await db.Questions.Where(q => q.ChapterId == chapterId).ToListAsync();
			if (ques.Count == 0)
				return null;
			Random rnd = new Random();
			Question rndQues;
			for (int i = 0; i < 3;)
			{
				rndQues = ques[rnd.Next(0, ques.Count)];
				if (rndQues.ques_type == ques_type.POL && !questions.Contains(rndQues))
				{
					questions.Add(rndQues);
					i++;
				}
			}
			for (int i = 0; i < 3;)
			{
				rndQues = ques[rnd.Next(0, ques.Count)];
				if (rndQues.ques_type == ques_type.CHL && !questions.Contains(rndQues))
				{
					questions.Add(rndQues);
					i++;
				}
			}
			for (int i = 0; i < 2;)
			{
				rndQues = ques[rnd.Next(0, ques.Count)];
				if (rndQues.ques_type == ques_type.UPR && !questions.Contains(rndQues))
				{
					questions.Add(rndQues);
					i++;
				}
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
