using API.Models;
using SAP2000v1;

namespace API.Services
{
    public class RebarMaterialBuilder : IMaterialBuilder
    {
        public void Build(cSapModel sapModel, IMaterialProperties material)
        {
            if (!(material is RebarMaterialProperties rebar)) return;

            string name = rebar.MaterialName;
            sapModel.PropMaterial.SetMaterial(name, rebar.MaterialType);

            // özelliklerini ata
            sapModel.PropMaterial.SetMPIsotropic(name, rebar.ModulusOfElasticity, rebar.PoissonRatio, rebar.ThermalCoeff);
            sapModel.PropMaterial.SetWeightAndMass(name, 1, rebar.UnitWeight);
            sapModel.PropMaterial.SetORebar_1(name, rebar.Fy, rebar.Fu, rebar.Fy, rebar.Fu, 1, 1, 0.02, 0.09, -0.1, false);
        }
    }
}
