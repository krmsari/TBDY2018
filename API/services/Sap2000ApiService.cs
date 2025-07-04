using EtabsApi.services.builders;
using API.Models;
using API.Models.Placements;
using API.Models.Seismic; // Yeni model için eklendi
using API.Services.Builders;
using API.Services.Builders.Sections;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Services
{
    public class Sap2000ApiService : ISap2000ApiService
    {
        public void CreateProjectInNewModel(GridSystemData gridData, SeismicParameters seismicParameters, List<IMaterialProperties> materials, List<ISectionProperties> sections, List<ColumnPlacementInfo> columnPlacements, List<BeamPlacementInfo> beamPlacements, bool makeVisible)
        {
            cOAPI sapObject = null;
            cSapModel sapModel = null;

            try
            {
                cHelper helper = new Helper();
                sapObject = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
                sapObject.ApplicationStart(eUnits.N_mm_C);
                sapModel = sapObject.SapModel;

                // 1. Grid Sistemini Oluştur
                if (gridData != null) new GridSystemBuilder().Build(sapModel, gridData);

                // 2. Yük Durumlarını Tanımla
                new LoadPatternBuilder(sapModel).DefineLoadPatterns();

                // 3. Otomatik Deprem Yükü Parametrelerini Güncelle
                if (seismicParameters != null) new SeismicLoadBuilder(sapModel).DefineSeismicLoads(seismicParameters);

                // 4. Malzemeleri Tanımla
                if (materials != null && materials.Any())
                {
                    foreach (var material in materials)
                    {
                        var builder = Sap2000MaterialBuilderFactory.GetBuilder(material.MaterialType);
                        builder.Build(sapModel, material);
                    }
                }

                // 5. Kesitleri Tanımla
                if (sections != null && sections.Any())
                {
                    foreach (var section in sections)
                    {
                        if (section is ColumnSectionProperties col) new ColumnBuilder().Build(sapModel, col);
                        else if (section is BeamSectionProperties beam) new BeamBuilder().Build(sapModel, beam);
                        else if (section is SlabSectionProperties slab) new SlabBuilder().Build(sapModel, slab);
                    }
                }

                // 6. Çubuk Elemanları Çiz
                var frameBuilder = new FrameObjectsBuilder(sapModel, columnPlacements, beamPlacements, gridData);
                frameBuilder.BuildAll();

                // 7. Temel Mesnetlerini Ata
                new RestraintBuilder(sapModel).supportJoints();

            }
            catch (Exception ex)
            {
                throw new Exception("SAP2000'e aktarımda hata oldu: \n" + ex.Message);
            }
            finally
            {
                if (sapObject != null && !makeVisible)
                {
                    sapObject.ApplicationExit(false);
                }
            }
        }
    }
}
