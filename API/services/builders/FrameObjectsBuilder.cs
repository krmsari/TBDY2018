using API.Models.Placements;
using SAP2000v1;
using System.Collections.Generic;
using System.Linq;

namespace API.Services.Builders
{
    /// <summary>
    /// Sorumluluğu, modeldeki kolon ve kiriş gibi çubuk elemanları çizmektir.
    /// </summary>
    public class FrameObjectsBuilder
    {
        private readonly cSapModel _sapModel;
        private readonly List<ColumnPlacementInfo> _columnPlacements;
        private readonly List<BeamPlacementInfo> _beamPlacements;
        private readonly List<double> _storyZCoordinates;

        public FrameObjectsBuilder(cSapModel sapModel, List<ColumnPlacementInfo> columnPlacements, List<BeamPlacementInfo> beamPlacements, GridSystemData gridData)
        {
            _sapModel = sapModel;
            _columnPlacements = columnPlacements;
            _beamPlacements = beamPlacements;
            _storyZCoordinates = gridData.ZCoordinates;
        }

        /// <summary>
        /// Tüm çubuk elemanları (kolonlar ve kirişler) oluşturur.
        /// </summary>
        public void BuildAll()
        {
            BuildColumns();
            BuildBeams();
        }

        /// <summary>
        /// Tanımlanan tüm kolonları her kat için çizer.
        /// </summary>
        private void BuildColumns()
        {
            if (_columnPlacements == null || !_columnPlacements.Any()) return;

            // Her kat aralığı için kolonları çiz (örneğin, Z=0'dan Z=3200'e, Z=3200'den Z=6400'e)
            for (int i = 0; i < _storyZCoordinates.Count - 1; i++)
            {
                double z1 = _storyZCoordinates[i];
                double z2 = _storyZCoordinates[i + 1];

                foreach (var colPlacement in _columnPlacements)
                {
                    string frameName = ""; // SAP2000'in otomatik isimlendirmesi için boş bırakılır
                    _sapModel.FrameObj.AddByCoord(
                        colPlacement.X, colPlacement.Y, z1, // Başlangıç noktası (Xi, Yi, Zi)
                        colPlacement.X, colPlacement.Y, z2, // Bitiş noktası (Xj, Yj, Zj)
                        ref frameName,
                        colPlacement.SectionName,
                        colPlacement.ColumnName + $"_Z{i + 1}", // Etiket: S101_Z1, S101_Z2 vb.
                        "Global"
                    );
                }
            }
        }

        /// <summary>
        /// Tanımlanan tüm kirişleri her kat seviyesinde çizer.
        /// </summary>
        private void BuildBeams()
        {
            if (_beamPlacements == null || !_beamPlacements.Any()) return;

            // Z=0 (temel) hariç her kat seviyesi için kirişleri çiz
            foreach (double z in _storyZCoordinates.Where(z => z > 0))
            {
                foreach (var beamPlacement in _beamPlacements)
                {
                    // Başlangıç ve bitiş kolonlarının koordinatlarını bul
                    var startCol = _columnPlacements.FirstOrDefault(c => c.ColumnName == beamPlacement.StartColumnName);
                    var endCol = _columnPlacements.FirstOrDefault(c => c.ColumnName == beamPlacement.EndColumnName);

                    if (startCol != null && endCol != null)
                    {
                        string frameName = "";
                        _sapModel.FrameObj.AddByCoord(
                            startCol.X, startCol.Y, z,   // Başlangıç noktası
                            endCol.X, endCol.Y, z,     // Bitiş noktası
                            ref frameName,
                            beamPlacement.SectionName,
                            beamPlacement.BeamName + $"_Z{z}", // Etiket
                            "Global"
                        );

                        if (!string.IsNullOrEmpty(frameName))
                        {
                            double[] doubles = new double[3];
                            _sapModel.FrameObj.SetInsertionPoint(frameName, 8, false, true, ref doubles, ref doubles);
                        }
                    }

                }
            }
        }
    }
}
