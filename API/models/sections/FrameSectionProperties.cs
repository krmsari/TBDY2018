namespace API.Models
{
    public class FrameSectionProperties
    {
        public string SectionName { get; set; }
        public string MaterialName { get; set; }
        public string RebarMaterialName { get; set; } 
        public double Depth { get; set; } // mm
        public double Width { get; set; } // mm
        public double ConcreteCover { get; set; } // mm
    }
}
