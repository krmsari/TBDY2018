using SAP2000v1;
using API.Models;

namespace API.Services
{
    public class FrameSectionBuilder : ISap2000Builder<FrameSectionProperties>
    {
        public void Build(cSapModel sapModel, FrameSectionProperties properties)
        {
            sapModel.PropFrame.SetRectangle(properties.SectionName, properties.MaterialName, properties.Depth, properties.Width);
        }
    }
}
