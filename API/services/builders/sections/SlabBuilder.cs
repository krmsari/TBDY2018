using SAP2000v1;
using API.Models;

namespace API.Services.Builders.Sections
{
    public class SlabBuilder : ISap2000Builder<SlabSectionProperties>
    {
        public void Build(cSapModel sapModel, SlabSectionProperties props)
        {
            double[] modifierForSlap = new double[] {
                0.25,
                0.25,
                0.25,
                0.25,
                0.25,
                0.25,
                1,
                1
            };
            sapModel.PropArea.SetModifiers(props.SectionName, ref modifierForSlap);
            sapModel.PropArea.SetShell(
                Name: props.SectionName,
                ShellType: 1, //
                MatProp: props.SlabMaterialName,
                MatAng: 0,
                Thickness: props.Thickness,
                Bending: 16,
                Color: 0, // Default color
                Notes: "", // No notes
                GUID: "" // No GUID
            );
        }
    }
}
