using API.Models;
using SAP2000v1;

namespace API.Services
{
    public class ConcreteMaterialBuilder : IMaterialBuilder
    {
        public void Build(cSapModel sapModel, IMaterialProperties material)
        {
            if (!(material is ConcreteMaterialProperties concrete)) return;

            string name = concrete.MaterialName;
            sapModel.PropMaterial.SetMaterial(name, concrete.MaterialType);

            // özelliklerini ata
            sapModel.PropMaterial.SetMPIsotropic(name, concrete.ModulusOfElasticity, concrete.PoissonRatio, concrete.ThermalCoeff);
            sapModel.PropMaterial.SetWeightAndMass(name, 1, concrete.UnitWeight);
            sapModel.PropMaterial.SetOConcrete_1(name, concrete.Fck, false, 1, 2, 4, 0.0022, 0.005, -0.1);
        }
    }
}
