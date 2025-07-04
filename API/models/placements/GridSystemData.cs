using System.Collections.Generic;

namespace API.Models.Placements
{
    public class GridSystemData
    {
        public List<double> XCoordinates { get; set; }
        public List<double> YCoordinates { get; set; }
        public List<double> ZCoordinates { get; set; }

        public GridSystemData()
        {
            XCoordinates = new List<double>();
            YCoordinates = new List<double>();
            ZCoordinates = new List<double>();
        }
    }
}
