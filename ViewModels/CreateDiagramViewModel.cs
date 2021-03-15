using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Models;

namespace TestingSystem.ViewModels
{
    public class CreateDiagramViewModel
    {
        public CreateDiagramViewModel()
        {
            Courses = new List<Course>();
        }
        public int ChaptersCount { get; set; }
        public double[,] Matrix { get; set; }
        public List<Course> Courses { get; set; }
    }
}
