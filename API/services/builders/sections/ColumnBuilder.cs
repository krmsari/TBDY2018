using SAP2000v1;
using API.Models;

namespace API.Services.Builders.Sections
{
    // Kolon kesitlerini ve donatılarını tanımlar.
    public class ColumnBuilder : ISap2000Builder<ColumnSectionProperties>
    {
        public void Build(cSapModel sapModel, ColumnSectionProperties props)
        {
            sapModel.PropFrame.SetRectangle(props.SectionName, props.MaterialName, props.Depth, props.Width, -2);

            double[] modifierForColumn = new double[] {
                1,
                1,
                1,
                1,
                0.7,
                0.7,
                1,
                1
            };

            sapModel.PropFrame.SetModifiers(props.SectionName, ref modifierForColumn);

            sapModel.PropFrame.SetRebarColumn(
                Name: props.SectionName,
                MatPropLong: props.RebarMaterialName,
                MatPropConfine: props.RebarMaterialName,
                Pattern: 1, // dikdörtgen
                ConfineType: 1, // etriyeli
                Cover: props.ConcreteCover,
                NumberCBars: 4,
                NumberR3Bars: 3,
                NumberR2Bars: 3,
                RebarSize: "16", // sembolik
                TieSize: "8",    // sembolik çünkü ToBeDesigned true değeri alyor
                TieSpacingLongit: 150,
                Number2DirTieBars: 2,
                Number3DirTieBars: 2,
                ToBeDesigned: true
            );
        }
    }
}
