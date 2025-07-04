using SAP2000v1;

namespace API.Models
{
    // Malzeme öz. tanımlamak için temel interface.
    public interface IMaterialProperties
    {
        string MaterialName { get; set; }
        eMatType MaterialType { get; }
        double ModulusOfElasticity { get; }
        double PoissonRatio { get; }
        double ThermalCoeff { get; }
        double UnitWeight { get; }
    }
}
