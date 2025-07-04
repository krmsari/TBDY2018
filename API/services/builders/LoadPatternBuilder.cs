using SAP2000v1;
using System;
using System.Collections.Generic;

namespace API.Services.Builders
{
    /// <summary>
    /// SAP2000 veritabanı tablolarını kullanarak yük durumlarını (Load Patterns) tanımlar.
    /// </summary>
    public class LoadPatternBuilder
    {
        private readonly cSapModel _sapModel;

        public LoadPatternBuilder(cSapModel sapModel)
        {
            this._sapModel = sapModel;
        }

        /// <summary>
        /// Proje için gerekli olan tüm yük durumlarını tanımlar.
        /// Mevcut "DEAD" yükünü günceller ve diğerlerini ekler.
        /// </summary>
        public void DefineLoadPatterns()
        {
            string tableName = "Load Pattern Definitions";
            int tableVersion = 0;
            string[] fields = null;
            int numRec = 0;
            string[] tableData = null;

            // 1. Tablo yapısını (sütun başlıklarını) al
            int ret = _sapModel.DatabaseTables.GetTableForEditingArray(tableName, "All", ref tableVersion, ref fields, ref numRec, ref tableData);
            if (ret != 0 || fields == null || fields.Length == 0)
            {
                throw new Exception($"SAP2000 '{tableName}' tablosu alınamadı.");
            }

            // 2. Tanımlanacak yük durumlarının listesini oluştur
            var loadPatternsToAdd = new List<Tuple<string, string, int, string>>
            {
                // LoadPat, DesignType, SelfWtMult, AutoLoad
                Tuple.Create("Ölü", "Dead", 1, ""),
                Tuple.Create("Hareketli", "Live", 0, ""),
                Tuple.Create("Ex", "Quake", 0, "TSC-2018"),
                Tuple.Create("Ex-Bod", "Quake", 0, "TSC-2018"),
                Tuple.Create("Ey", "Quake", 0, "TSC-2018"),
                Tuple.Create("Ey-Bod", "Quake", 0, "TSC-2018"),
                Tuple.Create("Ez", "Quake", 0, "TSC-2018"),
                Tuple.Create("+Wix", "Wind", 0, "TS 498-97"),
                Tuple.Create("-Wix", "Wind", 0, "TS 498-97"),
                Tuple.Create("+Wiy", "Wind", 0, "TS 498-97"),
                Tuple.Create("-Wiy", "Wind", 0, "TS 498-97"),
                Tuple.Create("Kar", "Snow", 0, "")
            };

            // 3. Yeni tablo verilerini oluştur
            var newTableDataList = new List<string>();
            int numCols = fields.Length;

            foreach (var lp in loadPatternsToAdd)
            {
                var rowData = new string[numCols];
                // Varsayılan değerleri ata
                for (int i = 0; i < rowData.Length; i++) { rowData[i] = ""; }

                rowData[Array.IndexOf(fields, "LoadPat")] = lp.Item1;
                rowData[Array.IndexOf(fields, "DesignType")] = lp.Item2;
                rowData[Array.IndexOf(fields, "SelfWtMult")] = lp.Item3.ToString();
                rowData[Array.IndexOf(fields, "AutoLoad")] = lp.Item4;

                newTableDataList.AddRange(rowData);
            }

            // 4. Güncellenmiş tabloyu SAP2000'e gönder
            int newNumRec = loadPatternsToAdd.Count;
            string[] newTableData = newTableDataList.ToArray();

            ret = _sapModel.DatabaseTables.SetTableForEditingArray(tableName, ref tableVersion, ref fields, newNumRec, ref newTableData);
            if (ret != 0)
            {
                throw new Exception($"SAP2000 '{tableName}' tablosu güncellenemedi. Hata kodu: " + ret);
            }

            // 5. Değişiklikleri uygula
            bool applyToAll = false;
            int fatalError = 0, numErrors = 0, numWarnings = 0, numInfo = 0;
            string msg = "";
            _sapModel.DatabaseTables.ApplyEditedTables(applyToAll, ref fatalError, ref numErrors, ref numWarnings, ref numInfo, ref msg);
            if (fatalError > 0 || numErrors > 0)
            {
                throw new Exception($"Yük durumları uygulanırken hata oluştu: {msg}");
            }
        }
    }
}
