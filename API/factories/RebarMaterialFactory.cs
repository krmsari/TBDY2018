using API.Models;
using System;
using System.Collections.Generic;

namespace API.Factories
{
    public class RebarMaterialFactory : IMaterialFactory
    {
        public IMaterialProperties CreateMaterial(Dictionary<string, object> parameters)
        {
            return new RebarMaterialProperties
            {
                MaterialName = (string)parameters["MaterialName"],
                Fy = Convert.ToDouble(parameters["Fy"]),
                Fu = Convert.ToDouble(parameters["Fu"]),
            };
        }
    }
}
