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
			return View();
		}

		public IActionResult StudentDiagram()
		{
			return View(Courses);
		}

		public IActionResult GroupDiagram()
		{
			return View(Courses);
		}

		public async Task<IActionResult> GetRandomData(string firstName, string lastName, int courseId)
		{
			Random rnd = new Random();
			var course = await db.Courses.FindAsync(courseId);
			if (course is null)
				return NotFound(new { Message = "Предмет не найден" });

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

		public async Task<IActionResult> GetStudentData(string firstName, string lastName,
			int courseId, string groupNumber)
		{
			var student = await db.Students
									.Include(s => s.Group)
									.Where(s => s.FirstName == firstName && s.LastName == lastName && s.Group.Number == groupNumber)
									.FirstOrDefaultAsync();

			var course = await db.Courses
									.Include(c => c.Chapters)
									.Where(c => c.Id == courseId)
									.FirstOrDefaultAsync();

			var courseChapters = course.Chapters.ToArray();

			if (student is null)
				return NotFound(new { message = "Студент не найден!" });
			else
			{
				var studentAnswers = await db.StudentAnswers
												.Include(sa => sa.Question)
												.ThenInclude(q => q.Chapter)
												.Where(sa => sa.StudentId == student.Id && sa.Question.Chapter.CourseId == courseId)
												.OrderByDescending(sa => sa.Date)
												.Take((course.ChaptersCount * 2) + (course.ChaptersCount * 6))
												.ToListAsync();
				int iterationCount = studentAnswers.Count / 8;
				if (studentAnswers is null || studentAnswers.Count == 0)
					return NotFound(new { message = "Данные об этом студенте не найдено" });
				double POL, UPR, CHL;
				double finalPOL = 1, finalUPR = 1, finalCHL = 1;
				double[][] matrix = new double[course.ChaptersCount + 1][];

				for (int i = 0; i < matrix.Length; i++)
				{
					matrix[i] = new double[4];
				}

				for (int i = 0; i < iterationCount + 1; i++)
				{
					matrix[i] = new double[4];
					if (i == iterationCount)
					{
						if (iterationCount < course.ChaptersCount)
							continue;
						matrix[courseChapters.Length][0] = Math.Pow(finalUPR, 1d / iterationCount);
						matrix[courseChapters.Length][1] = Math.Pow(finalCHL, 1d / iterationCount);
						matrix[courseChapters.Length][2] = Math.Pow(finalPOL, 1d / iterationCount);
						double a = 1.0d;
						for (int j = 0; j < iterationCount; j++)
						{
							a *= matrix[j][3];
						}
						matrix[courseChapters.Length][3] = Math.Pow(a, 1d / iterationCount);
					}
					else
					{
						UPR = studentAnswers
								.Where(sa => sa.Question.ques_type == ques_type.UPR && sa.Question.ChapterId == courseChapters[i].Id)
								.Sum(sa => sa.Point);
						CHL = studentAnswers
									.Where(sa => sa.Question.ques_type == ques_type.CHL && sa.Question.ChapterId == courseChapters[i].Id)
									.Sum(sa => sa.Point);
						POL = studentAnswers
									.Where(sa => sa.Question.ques_type == ques_type.POL && sa.Question.ChapterId == courseChapters[i].Id)
									.Sum(sa => sa.Point);

						matrix[i][0] = UPR / 8;
						matrix[i][1] = CHL / 6;
						matrix[i][2] = POL / 3;
						Correct(matrix, i);
						matrix[i][3] = Math.Pow(matrix[i][0] * matrix[i][1] * matrix[i][2], 1d / 3);
						finalUPR *= matrix[i][0];
						finalCHL *= matrix[i][1];
						finalPOL *= matrix[i][2];
					}
				}
				return Json(new { chaptersCount = course.ChaptersCount, matrix = matrix, showFinalDiagram = !(iterationCount < course.ChaptersCount) });
			}
		}

		private async Task<StudentDTO> GetStudent(int studentId, int courseId, int iterationCount)
		{
			var student = await db.Students.FindAsync(studentId);

			var course = await db.Courses
									.Include(c => c.Chapters)
									.Where(c => c.Id == courseId)
									.FirstOrDefaultAsync();

			var courseChapters = course.Chapters.ToArray();

			if (student is null)
				return null;
			else
			{
				var studentAnswers = await db.StudentAnswers
												.Include(sa => sa.Question)
												.ThenInclude(sa => sa.Chapter)
												.Where(sa => sa.StudentId == student.Id && sa.Question.Chapter.CourseId == courseId)
												.OrderByDescending(sa => sa.Date)
												.Take((iterationCount * 2) + (iterationCount * 6))
												.ToListAsync();

				double POL, UPR, CHL;
				double finalPOL = 1, finalUPR = 1, finalCHL = 1;
				double[][] matrix = new double[course.ChaptersCount + 1][];
				for (int i = 0; i < matrix.Length; i++)
				{
					matrix[i] = new double[4];
				}

				for (int i = 0; i < iterationCount + 1; i++)
				{
					if (i == iterationCount)
					{
						matrix[courseChapters.Length][0] = Math.Pow(finalUPR, 1d / iterationCount);
						matrix[courseChapters.Length][1] = Math.Pow(finalCHL, 1d / iterationCount);
						matrix[courseChapters.Length][2] = Math.Pow(finalPOL, 1d / iterationCount);
						double a = 1;
						for (int j = 0; j < iterationCount; j++)
						{
							a *= matrix[j][3];
						}
						matrix[courseChapters.Length][3] = Math.Pow(a, 1d / iterationCount);
					}
					else
					{
						UPR = studentAnswers
								.Where(sa => sa.Question.ques_type == ques_type.UPR && sa.Question.ChapterId == courseChapters[i].Id)
								.Sum(sa => sa.Point);
						CHL = studentAnswers
									.Where(sa => sa.Question.ques_type == ques_type.CHL && sa.Question.ChapterId == courseChapters[i].Id)
									.Sum(sa => sa.Point);
						POL = studentAnswers
									.Where(sa => sa.Question.ques_type == ques_type.POL && sa.Question.ChapterId == courseChapters[i].Id)
									.Sum(sa => sa.Point);

						matrix[i][0] = UPR / 8;
						matrix[i][1] = CHL / 6;
						matrix[i][2] = POL / 3;
						Correct(matrix, i);
						matrix[i][3] = Math.Pow(matrix[i][0] * matrix[i][1] * matrix[i][2], 1d / 3);
						finalUPR *= matrix[i][0];
						finalCHL *= matrix[i][1];
						finalPOL *= matrix[i][2];
					}
				}
				return new StudentDTO() { FirstName = student.FirstName, LastName = student.LastName, Matrix = matrix };
			}
		}

		public async Task<IActionResult> GetGroupData(string groupNumber, int courseId)
		{
			var studentAnswers = await db.StudentAnswers
				.Include(sa => sa.Student)
				.ThenInclude(s => s.Group)
				.Include(sa => sa.Question)
				.ThenInclude(q => q.Chapter)
				.ThenInclude(c => c.Course)
				.Where(sa => sa.Student.Group.Number == groupNumber && sa.Question.Chapter.CourseId == courseId)
				.Distinct()
				.ToListAsync();

			var students = db.Students.Include(s => s.Group).Where(s => s.Group.Number == groupNumber).ToList();
			int max = 0;
			int k;
			for (int i = 0; i < students.Count; i++)
			{
				k = 0;
				for (int j = 0; j < studentAnswers.Count; j++)
				{
					if (studentAnswers[j].StudentId == students[i].Id)
						k++;
				}
				max = k > max ? k : max;
			}
			int iterationCount = max / 8;
			var studentsId = studentAnswers.Select(s => s.Student.Id).Distinct().ToList();

			StudentDTO student;
			StudentViewModel vm = new StudentViewModel() { ChaptersCount = (await db.Courses.FindAsync(courseId)).ChaptersCount };
			for (int i = 0; i < studentsId.Count; i++)
			{
				student = await GetStudent(studentsId[i], courseId, iterationCount);
				vm.Students.Add(await GetStudent(studentsId[i], courseId, iterationCount));
			}
			Calculate(vm);
			return Json(vm);
		}

		private double Gauss(double z)
		{
			// input = z-value (-inf to +inf)
			// output = p under Standard Normal curve from -inf to z
			// e.g., if z = 0.0, function returns 0.5000
			// ACM Algorithm #209
			double y; // 209 scratch variable
			double p; // result. called 'z' in 209
			double w; // 209 scratch variable
			if (z == 0.0)
				p = 0.0;
			else
			{
				y = Math.Abs(z) / 2;
				if (y >= 3.0)
				{
					p = 1.0;
				}
				else if (y < 1.0)
				{
					w = y * y;
					p = ((((((((0.000124818987 * w
					- 0.001075204047) * w + 0.005198775019) * w
					- 0.019198292004) * w + 0.059054035642) * w
					- 0.151968751364) * w + 0.319152932694) * w
					- 0.531923007300) * w + 0.797884560593) * y * 2.0;
				}
				else
				{
					y = y - 2.0;
					p = (((((((((((((-0.000045255659 * y
					+ 0.000152529290) * y - 0.000019538132) * y
					- 0.000676904986) * y + 0.001390604284) * y
					- 0.000794620820) * y - 0.002034254874) * y
					+ 0.006549791214) * y - 0.010557625006) * y
					+ 0.011630447319) * y - 0.009279453341) * y
					+ 0.005353579108) * y - 0.002141268741) * y
					+ 0.000535310849) * y + 0.999936657524;
				}
			}
			if (z > 0.0)
				return (p + 1.0) / 2;
			else
				return (1.0 - p) / 2;
		}

		private void Calculate(StudentViewModel vm)
		{
			double matOj = 0;
			double disp = 0;
			vm.N = new double[5];
			vm.P = new double[5];
			for (int i = 0; i < vm.Students.Count; i++)
			{
				matOj += vm.Students[i].Matrix[3][3];
				if (vm.Students[i].Matrix[3][3] > 0 && vm.Students[i].Matrix[3][3] <= 0.2)
					vm.N[0]++;
				else if (vm.Students[i].Matrix[3][3] > 0.2 && vm.Students[i].Matrix[3][3] <= 0.4)
					vm.N[1]++;
				else if (vm.Students[i].Matrix[3][3] > 0.4 && vm.Students[i].Matrix[3][3] <= 0.6)
					vm.N[2]++;
				else if (vm.Students[i].Matrix[3][3] > 0.6 && vm.Students[i].Matrix[3][3] <= 0.8)
					vm.N[3]++;
				else if (vm.Students[i].Matrix[3][3] > 0.8 && vm.Students[i].Matrix[3][3] <= 1)
					vm.N[4]++;
			}
			vm.MatOj = matOj / vm.Students.Count;
			for (int i = 0; i < vm.Students.Count; i++)
			{
				disp += Math.Pow(vm.Students[i].Matrix[3][3], 2) / vm.Students.Count;
			}
			disp -= Math.Pow(vm.MatOj, 2);
			vm.Otkl = Math.Sqrt(disp);
			for (int i = 0; i < 5; i++)
			{
				vm.P[i] = Gauss((0.2 + i / 5d - vm.MatOj) / vm.Otkl) - Gauss((i / 5d - vm.MatOj) / vm.Otkl);
				if (vm.P[i] < 0.0001)
					vm.P[i] = 0.0001;
				vm.X += Math.Pow(vm.N[i] - (5 * vm.P[i]), 2) / (5 * vm.P[i]);
			}
			vm.Disp = disp;
		}

		private void Correct(double[][] matrix, int i)
		{
			matrix[i][0] = matrix[i][0] < 0.0001 ? 0.0001 : matrix[i][0];
			matrix[i][1] = matrix[i][1] < 0.0001 ? 0.0001 : matrix[i][1];
			matrix[i][2] = matrix[i][2] < 0.0001 ? 0.0001 : matrix[i][2];
		}
	}
}
