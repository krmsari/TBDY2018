using SAP2000v1;

namespace API.Services
{
    // ETABS nesnesini temel arayüz
    public interface ISap2000Builder<T>
    {
        void Build(cSapModel sapModel, T properties);
    }
}
