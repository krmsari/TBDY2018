using SAP2000v1;
using API.Models;

namespace API.Services.Builders.Sections
{
    public class BeamBuilder : ISap2000Builder<BeamSectionProperties>
    {
        public void Build(cSapModel sapModel, BeamSectionProperties props)
        {
            sapModel.PropFrame.SetRectangle(props.SectionName, props.MaterialName, props.Depth, props.Width,3);

            double[] modifierForBeam = new double[] {
                1,
                1,
                1,
                1,
                0.35,
                0.35,
                1,
                1
            };

            sapModel.PropFrame.SetModifiers(props.SectionName, ref modifierForBeam);

            sapModel.PropFrame.SetRebarBeam(
                Name: props.SectionName,
                MatPropLong: props.RebarMaterialName,
                MatPropConfine: props.RebarMaterialName,
                CoverTop: props.CoverTop,
                CoverBot: props.CoverBottom,
                TopLeftArea: 100,   
                TopRightArea: 100, 
                BotLeftArea: 100,  
                BotRightArea: 100  
            );
        }
    }
}
