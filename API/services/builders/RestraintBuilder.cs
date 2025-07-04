
using API.Models.Placements;
using SAP2000v1;
using System;
using System.Collections.Generic;

namespace EtabsApi.services.builders
{
    class RestraintBuilder
    {
        cSapModel sapModel;
        public RestraintBuilder(cSapModel sapModel) 
        {
            this.sapModel = sapModel;
        }

        public List<string> finder()
        {
            string tableName = "Joint Coordinates";
            int tableVersion = 0;
            string[] fields = null;
            int numRec = 0;
            string[] tableData = null;

            int ret = sapModel.DatabaseTables.GetTableForEditingArray(tableName, "All", ref tableVersion, ref fields, ref numRec, ref tableData);
            if (ret != 0 || fields == null || fields.Length == 0)
            {
                throw new Exception($"SAP2000 '{tableName}' tablosu alınamadı. Model doğru başlatılamamış olabilir.");
            }
            int checkColumnIndex = Array.IndexOf(fields, "Z");
            int returnColumnIndex = Array.IndexOf(fields, "Joint");
            string checkValue = "0";
            int columncount = fields.Length;

            List<string> resultList = new List<string>();

            if (checkColumnIndex == -1 || returnColumnIndex == -1)
            {
                Console.WriteLine($"Hata: Z veya Joint sütunlarından biri bulunamadı.");
            }
            for(int i = 0; i < tableData.Length; i += columncount)
            {
                string zValue = tableData[i + checkColumnIndex];
                string jointValue = tableData[i + returnColumnIndex];

                if (zValue == checkValue)
                {
                    // Koşul sağlanıyorsa, Joint değerini listeye ekle.
                    resultList.Add(jointValue);
                }
            }
            Console.WriteLine(tableData);
            return resultList;
        }

        public void supportJoints()
        {
            bool[] restraints = new bool[6] { true, true, true, true, true, true };
            foreach (var joint in finder())
            {
                sapModel.PointObj.SetRestraint(joint,ref restraints);
            }
        }
    }
}
