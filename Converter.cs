using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DosToWin
{

    public static class DataTableConverter
    {
        public static void ToCSV(DataTable table, string filename, Encoding encoding, string delimeter, bool saveHeader)
        {
            if (table == null) return;
            StreamWriter sw = new StreamWriter(filename, false, encoding);

            string line = "sep=;" + Environment.NewLine; 
            if (saveHeader)
            {
                for (int i = 0; i < table.Columns.Count; i++) line = line + table.Columns[i].ColumnName.ToString() + delimeter;
                line = line.Substring(0, line.Length - 1);
                sw.WriteLine(line);
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                line = "";
                for (int j = 0; j < table.Columns.Count; j++) line = line +  "\t" + table.Rows[i].ItemArray[j].ToString() + delimeter;
                line = line.Substring(0, line.Length - 1);
                sw.WriteLine(line);
                sw.Flush();
            }

            sw.Close();
        }
         
        
        
        
    }
}
