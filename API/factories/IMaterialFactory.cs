using API.Models;
using System.Collections.Generic;

namespace API.Factories
{
    public interface IMaterialFactory
    {
        IMaterialProperties CreateMaterial(Dictionary<string, object> parameters);
    }
}
