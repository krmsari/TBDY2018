using API.Models;
using System;
using System.Collections.Generic;

namespace API.Factories
{
    public class ConcreteMaterialFactory : IMaterialFactory
    {
        public IMaterialProperties CreateMaterial(Dictionary<string, object> parameters)
        {
            return new ConcreteMaterialProperties
            {
                MaterialName = (string)parameters["MaterialName"],
                Fck = Convert.ToDouble(parameters["Fck"]),
            };
        }
    }
}
