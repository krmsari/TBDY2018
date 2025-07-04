using SAP2000v1;
using System;

namespace API.Services
{
    // malzeme tipine göre doğru 'builder' sınıfını seçer
    public static class Sap2000MaterialBuilderFactory
    {
        public static IMaterialBuilder GetBuilder(eMatType type)
        {
            switch (type)
            {
                case eMatType.Concrete: return new ConcreteMaterialBuilder();
                case eMatType.Rebar: return new RebarMaterialBuilder();
                default: throw new NotSupportedException("Desteklenmeyen malzeme tipi: " + type);
            }
        }
    }
}
