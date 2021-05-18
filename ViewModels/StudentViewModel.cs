using System.Collections.Generic;

namespace TestingSystem.ViewModels
{
    public class StudentViewModel
    {
        public StudentViewModel()
        {
            Students = new List<StudentDTO>();
        }
        public List<StudentDTO> Students { get; set; }
        //public double[][] ChartMatrix { get; set; }
        public int ChaptersCount { get; set; }
        public double MatOj { get; set; }
        public double Disp { get; set; }
        public double Otkl { get; set; }
        public double X { get; set; }
        public double[] P { get; set; }
        public double[] N { get; set; }
    }
}
