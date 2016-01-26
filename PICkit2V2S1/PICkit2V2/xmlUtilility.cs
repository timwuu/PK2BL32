using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DTBL = System.Data.DataTable;


namespace PICkit2V2
{
    class xmlUtilility
    {
        private static void setupTblScripts(DTBL tblScripts)
        {
            tblScripts.Columns.Add("ScriptNumber", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("ScriptName", System.Type.GetType("System.String"));
            tblScripts.Columns.Add("ScriptVersion", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("UNUSED1", System.Type.GetType("System.UInt32"));
            tblScripts.Columns.Add("ScriptLength", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("Script", System.Type.GetType("System.UInt16[]"));
            tblScripts.Columns.Add("Comment", System.Type.GetType("System.String"));
        }


        public static void exportTblScripts(DeviceFile.DeviceScripts[] fScripts, String filename)
        {
            DTBL tblScripts= new DTBL("Scripts");
            setupTblScripts( tblScripts);

            System.Data.DataRow myNewRow;

            for (int i = 0; i < fScripts.Length; i++)
            {
                myNewRow = tblScripts.NewRow();

                myNewRow["ScriptNumber"] = fScripts[i].ScriptNumber; //UInt16();
                myNewRow["ScriptName"] = fScripts[i].ScriptName; //String();
                myNewRow["ScriptVersion"] = fScripts[i].ScriptVersion; //UInt16();
                myNewRow["UNUSED1"] = fScripts[i].UNUSED1; //UInt32();
                myNewRow["ScriptLength"] = fScripts[i].ScriptLength; //UInt16();
                myNewRow["Script"] = fScripts[i].Script;  //Unit16[];
                myNewRow["Comment"] = fScripts[i].Comment; //String();

                tblScripts.Rows.Add(myNewRow);
            }

            tblScripts.WriteXml(filename, System.Data.XmlWriteMode.WriteSchema);
        }

    }
}
