using API.Models.Placements;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace API.Services.Builders
{
    /// <summary>
    /// SAP2000 veritabanı tablolarını kullanarak özel bir grid sistemi oluşturur.
    /// </summary>
    public class GridSystemBuilder : ISap2000Builder<GridSystemData>
    {
        public void Build(cSapModel sapModel, GridSystemData gridData)
        {
            // 1. "Grid Lines" tablosunu oluşturmak için geçici bir model oluştur.
            // Bu, tabloyu düzenlememize olanak tanır.
            sapModel.File.NewSolidBlock(0, 0, 0,true,"Defult",0,0,0);
            
            // 2. Grid tablosunun yapısını (sütun başlıklarını) al.
            string tableName = "Grid Lines";
            int tableVersion = 0;
            string[] fields = null;
            int numRec = 0;
            string[] tableData = null;

            int ret = sapModel.DatabaseTables.GetTableForEditingArray(tableName, "All", ref tableVersion, ref fields, ref numRec, ref tableData);
            if (ret != 0 || fields == null || fields.Length == 0)
            {
                throw new Exception($"SAP2000 '{tableName}' tablosu alınamadı. Model doğru başlatılamamış olabilir.");
            }

            // 3. Yeni grid verilerini oluştur.
            var newTableDataList = new List<string>();
            int numCols = fields.Length;

            // X Eksenindeki Gridleri Ekle
            for (int i = 0; i < gridData.XCoordinates.Count; i++)
            {
                string gridId = Convert.ToChar('A' + i).ToString(); // A, B, C...
                newTableDataList.AddRange(CreateGridRow(fields, "X", gridId, gridData.XCoordinates[i]));
            }

            // Y Eksenindeki Gridleri Ekle
            for (int i = 0; i < gridData.YCoordinates.Count; i++)
            {
                string gridId = (i + 1).ToString(); // 1, 2, 3...
                newTableDataList.AddRange(CreateGridRow(fields, "Y", gridId, gridData.YCoordinates[i]));
            }

            // Z Eksenindeki Gridleri Ekle (Kat Yükseklikleri)
            for (int i = 0; i < gridData.ZCoordinates.Count; i++)
            {
                string gridId = $"Z{i}"; // Z0, Z1, Z2...
                newTableDataList.AddRange(CreateGridRow(fields, "Z", gridId, gridData.ZCoordinates[i]));
            }

            // 4. Güncellenmiş tabloyu SAP2000'e gönder.
            int newNumRec = newTableDataList.Count / numCols;
            string[] newTableData = newTableDataList.ToArray();

            ret = sapModel.DatabaseTables.SetTableForEditingArray(tableName, ref tableVersion, ref fields, newNumRec, ref newTableData);
            if (ret != 0)
            {
                throw new Exception("SAP2000 grid tablosu güncellenemedi. Hata kodu: " + ret);
            }

            // 5. Değişiklikleri uygula ve modeli yenile.
            bool applyToAll = false; // Sadece bu tabloyu etkile
            int fatalError = 0, numErrors = 0, numWarnings = 0, numInfo = 0;
            string msg = "";
            sapModel.DatabaseTables.ApplyEditedTables(applyToAll, ref fatalError, ref numErrors, ref numWarnings, ref numInfo, ref msg);
            if (fatalError > 0 || numErrors > 0)
            {
                throw new Exception($"Grid sistemi uygulanırken hata oluştu: {msg}");
            }

            sapModel.View.RefreshView();
        }

        /// <summary>
        /// "Grid Lines" tablosu için tek bir satır verisi oluşturur.
        /// </summary>
        private string[] CreateGridRow(string[] fields, string axisDir, string gridId, double coordinate)
        {
            var rowData = new string[fields.Length];
            // Varsayılan değerleri atayarak null referans hatalarını önle
            for (int i = 0; i < rowData.Length; i++)
            {
                rowData[i] = "";
            }

            // Gerekli alanları doldur
            rowData[Array.IndexOf(fields, "CoordSys")] = "GLOBAL";
            rowData[Array.IndexOf(fields, "AxisDir")] = axisDir;
            rowData[Array.IndexOf(fields, "GridID")] = gridId;
            rowData[Array.IndexOf(fields, "XRYZCoord")] = coordinate.ToString(CultureInfo.InvariantCulture);
            rowData[Array.IndexOf(fields, "LineType")] = "Primary";
            rowData[Array.IndexOf(fields, "Visible")] = "Yes";
            rowData[Array.IndexOf(fields, "BubbleLoc")] = "End";

            return rowData;
        }
    }
}
