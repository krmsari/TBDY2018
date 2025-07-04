namespace API.Models
{
    public abstract class BaseFrameSectionProperties : ISectionProperties
    {
        public string SectionName { get; set; }
        public string MaterialName { get; set; }
        public double Depth { get; set; }
        public double Width { get; set; }
    }
}
