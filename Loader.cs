using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DosToWin
{
    static class Loader
    {
        public static DataTable FromDBF(string filepath, System.Text.Encoding readingencoding)
        { 

            try
            {
                //regular files like Standard.Dbf
                string strAccessConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath; ;
                string strAccessSelect = "SELECT * FROM " + Path.GetFileNameWithoutExtension(filepath);
                DataSet myDataSet = new DataSet();
                OleDbConnection myAccessConn = null;

                myAccessConn = new OleDbConnection(strAccessConn);
                OleDbCommand myAccessCommand = new OleDbCommand(strAccessSelect, myAccessConn);
                OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(myAccessCommand);

                myAccessConn.Open();
                myDataAdapter.Fill(myDataSet);

                myAccessConn.Close();

                return myDataSet.Tables[0];
            }
            catch
            {
                try
                {
                    //unregular files like Details.SBF, Bank.Dbf
                    return ParseDBF.ReadDBF(filepath, readingencoding);
                }
                catch (Exception e)
                {
                    //WTF !? column not found ?!
                    //MessageBox.Show(e.Message, "error");
                    return null;
                }
            }
        }
         
         
         
         
    }
}
