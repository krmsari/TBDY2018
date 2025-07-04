using API.Models;
using API.Models.Placements;
using API.Models.Seismic;
using System.Collections.Generic;

namespace API.Services
{
    public interface ISap2000ApiService
    {
        void CreateProjectInNewModel(
            GridSystemData gridData,
            SeismicParameters seismicParameters, // Yeni parametre
            List<IMaterialProperties> materials,
            List<ISectionProperties> sections,
            List<ColumnPlacementInfo> columnPlacements,
            List<BeamPlacementInfo> beamPlacements,
            bool makeVisible);
    }
}
