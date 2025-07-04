using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Services
{
    /// <summary>
    /// TBDY-2018 Tablo 2.1 ve 2.2'ye göre Fs ve F1 zemin etki katsayılarını hesaplar.
    /// Ara değerler için doğrusal interpolasyon yapar.
    /// </summary>
    public class TBDY2018CoefficientService
    {
        private readonly Dictionary<string, Dictionary<double, double>> _fsTable;
        private readonly Dictionary<string, Dictionary<double, double>> _f1Table;

        public TBDY2018CoefficientService()
        {
            // JSON verisi projeye gömülmüştür.
            string jsonData = @"
            {
              ""Fs_Table"": {
                ""ZA"": { ""0.25"": 0.8, ""0.50"": 0.8, ""0.75"": 0.8, ""1.00"": 0.8, ""1.25"": 0.8, ""1.50"": 0.8 },
                ""ZB"": { ""0.25"": 0.9, ""0.50"": 0.9, ""0.75"": 0.9, ""1.00"": 0.9, ""1.25"": 0.9, ""1.50"": 0.9 },
                ""ZC"": { ""0.25"": 1.3, ""0.50"": 1.3, ""0.75"": 1.2, ""1.00"": 1.2, ""1.25"": 1.2, ""1.50"": 1.2 },
                ""ZD"": { ""0.25"": 1.6, ""0.50"": 1.4, ""0.75"": 1.2, ""1.00"": 1.1, ""1.25"": 1.0, ""1.50"": 1.0 },
                ""ZE"": { ""0.25"": 2.4, ""0.50"": 1.7, ""0.75"": 1.3, ""1.00"": 1.1, ""1.25"": 0.9, ""1.50"": 0.8 }
              },
              ""F1_Table"": {
                ""ZA"": { ""0.10"": 0.8, ""0.20"": 0.8, ""0.30"": 0.8, ""0.40"": 0.8, ""0.50"": 0.8, ""0.60"": 0.8 },
                ""ZB"": { ""0.10"": 0.8, ""0.20"": 0.8, ""0.30"": 0.8, ""0.40"": 0.8, ""0.50"": 0.8, ""0.60"": 0.8 },
                ""ZC"": { ""0.10"": 1.5, ""0.20"": 1.5, ""0.30"": 1.5, ""0.40"": 1.5, ""0.50"": 1.5, ""0.60"": 1.4 },
                ""ZD"": { ""0.10"": 2.4, ""0.20"": 2.2, ""0.30"": 2.0, ""0.40"": 1.9, ""0.50"": 1.8, ""0.60"": 1.7 },
                ""ZE"": { ""0.10"": 4.2, ""0.20"": 3.3, ""0.30"": 2.8, ""0.40"": 2.4, ""0.50"": 2.2, ""0.60"": 2.0 }
              }
            }";

            var tables = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, double>>>>(jsonData);
            _fsTable = tables["Fs_Table"].ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(subKvp => double.Parse(subKvp.Key), subKvp => subKvp.Value));
            _f1Table = tables["F1_Table"].ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(subKvp => double.Parse(subKvp.Key), subKvp => subKvp.Value));
        }

        public (double Fs, double F1) CalculateCoefficients(double ss, double s1, string siteClass)
        {
            if (siteClass == "ZF")
            {
                throw new NotSupportedException("ZF zemin sınıfı için sahaya özel analiz gereklidir.");
            }
            double fs = Interpolate(_fsTable[siteClass], ss);
            double f1 = Interpolate(_f1Table[siteClass], s1);
            return (fs, f1);
        }

        private double Interpolate(Dictionary<double, double> table, double value)
        {
            var sortedKeys = table.Keys.OrderBy(k => k).ToList();
            if (value <= sortedKeys.First()) return table[sortedKeys.First()];
            if (value >= sortedKeys.Last()) return table[sortedKeys.Last()];

            int i = sortedKeys.FindIndex(k => k >= value);
            double x1 = sortedKeys[i - 1];
            double y1 = table[x1];
            double x2 = sortedKeys[i];
            double y2 = table[x2];

            // Doğrusal interpolasyon formülü: y = y1 + (x - x1) * (y2 - y1) / (x2 - x1)
            return y1 + (value - x1) * (y2 - y1) / (x2 - x1);
        }
    }
}
