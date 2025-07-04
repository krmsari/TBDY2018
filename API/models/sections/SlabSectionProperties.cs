using SAP2000v1;

namespace API.Models
{
    public class SlabSectionProperties : ISectionProperties
    {
        public string SectionName { get; set; }
        public string SlabMaterialName { get; set; }
        public double Thickness { get; set; }
        //public eSlabType SlabType { get; set; } = eSlabType.Slab;
    }
}
