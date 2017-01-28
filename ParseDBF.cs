using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DosToWin
{

    public class ParseDBF
    {


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFHeader
        {
            public byte version;
            public byte updateYear;
            public byte updateMonth;
            public byte updateDay;
            public Int32 numRecords;
            public Int16 headerLen;
            public Int16 recordLen;
            public Int16 reserved1;
            public byte incompleteTrans;
            public byte encryptionFlag;
            public Int32 reserved2;
            public Int64 reserved3;
            public byte MDX;
            public byte language;
            public Int16 reserved4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct FieldDescriptor
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public string fieldName;
            public char fieldType;
            public Int32 address;
            public byte fieldLen;
            public byte count;
            public Int16 reserved1;
            public byte workArea;
            public Int16 reserved2;
            public byte flag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] reserved3;
            public byte indexFlag;
        }

        public static DataTable ReadDBF(string dbfFile, System.Text.Encoding readingEncoding)
        {
            byte[] hrhSuggestedCharMap = new byte[] {48,49,50,51,52,53,54,55,56,57,161,220,190,194,198,198,199,199,200,200,129,129,202,202,203,203,204,204,141,141,205,205,206,206,
    207,208,209,210,142,211,211,212,212,213,213,214,214,216,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,45,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
    0,0,0,0,0,0,0,0,0,0,0,0,0,217,218,218,218,218,219,219,219,219,221,221,222,222,223,223,144,144,225,225,225,227,227,228,228,230,
    229,229,229,237,237,237,0 };

            long start = DateTime.Now.Ticks;
            DataTable dt = new DataTable();
            BinaryReader recReader;
            string number;
            string year;
            string month;
            string day;
            long lDate;
            long lTime;
            DataRow row;
            int fieldIndex;

            if ((false == File.Exists(dbfFile))) return dt;

            bool bad = false;
            BinaryReader br = null;
            try
            {
                br = new BinaryReader(File.OpenRead(dbfFile), readingEncoding.CodePage == 1256 ? Encoding.ASCII : readingEncoding);
                byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                DBFHeader header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
                handle.Free();

                ArrayList fields = new ArrayList();
                while (true)
                {
                    if (13 == br.PeekChar()) break;
                    if (-1 == br.PeekChar()) break;
                    buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
                    handle.Free();
                }

                ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
                buffer = br.ReadBytes(header.recordLen);
                recReader = new BinaryReader(new MemoryStream(buffer), readingEncoding.CodePage == 1256 ? Encoding.ASCII : readingEncoding);

                DataColumn col = null;
                foreach (FieldDescriptor field in fields)
                {
                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                    switch (field.fieldType)
                    {
                        case 'N':
                            //if (number.IndexOf(".") > -1)
                            {
                                col = new DataColumn(field.fieldName, typeof(decimal));
                            }
                            //else
                            //{
                            //    col = new DataColumn(field.fieldName, typeof(int));
                            //}
                            break;
                        case 'C':
                            col = new DataColumn(field.fieldName, typeof(string));
                            break;
                        case 'T':
                            col = new DataColumn(field.fieldName, typeof(DateTime));
                            break;
                        case 'D':
                            col = new DataColumn(field.fieldName, typeof(DateTime));
                            break;
                        case 'L':
                            col = new DataColumn(field.fieldName, typeof(bool));
                            break;
                        case 'F':
                            col = new DataColumn(field.fieldName, typeof(Double));
                            break;
                    }
                    try
                    {
                        dt.Columns.Add(col);
                    }
                    catch (Exception ee1)
                    {
                        try
                        {
                            continue;
                        }
                        catch
                        {
                            throw new Exception(ee1.Message);
                        }
                    }
                }

                ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);
                for (int counter = 0; counter <= header.numRecords - 1; counter++)
                {

                    buffer = br.ReadBytes(header.recordLen);
                    recReader = new BinaryReader(new MemoryStream(buffer), readingEncoding.CodePage == 1256 ? Encoding.ASCII : readingEncoding);

                    if (recReader.ReadChar() == '*') continue;

                    fieldIndex = 0;
                    row = dt.NewRow();
                    foreach (FieldDescriptor field in fields)
                    {
                        switch (field.fieldType)
                        {
                            case 'N':  // Number
                                try
                                {
                                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        //if (number.IndexOf(".") > -1)
                                        {
                                            row[fieldIndex] = decimal.Parse(number);
                                        }
                                        //else
                                        //{
                                        //    try
                                        //    {
                                        //        row[fieldIndex] = int.Parse(number);
                                        //    }
                                        //    catch
                                        //    {
                                        //        row[fieldIndex] = Convert.ToInt32(ulong.Parse(number));
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0;
                                    }
                                }
                                catch
                                {
                                    bad = true;
                                }
                                break;

                            case 'C': // String
                                if (readingEncoding.CodePage == 1256)
                                {
                                    byte[] b = recReader.ReadBytes(field.fieldLen);
                                    byte[] b2 = new byte[b.Length];
                                    for (int iii = 0; iii < b.Length; iii++)
                                        b2[iii] = b[b.Length - iii - 1];
                                    string t = Encoding.GetEncoding(1256).GetString(b2);
                                    //row[fieldIndex] = t;
                                    /*t = t.Replace('Ç', 'ا').Replace('È', 'ب').Replace('Â', 'آ').Replace('Æ', 'ئ').Replace('Á', 'ء').Replace('í', 'ي').Replace('å', 'ه')
                                     .Replace('', 'پ').Replace('Ê', 'ت').Replace('Ë', 'ث').Replace('Ì', 'ج').Replace('', 'چ').Replace('Í', 'ح').Replace('Î', 'خ')
                                     .Replace('Ï', 'د').Replace('Ð', 'ذ').Replace('Ñ', 'ر').Replace('Ò', 'ز').Replace('Ž', 'ژ').Replace('Ó', 'س').Replace('Ô', 'ش')
                                     .Replace('Õ', 'ص').Replace('Ö', 'ض').Replace('Ø', 'ط').Replace('Ù', 'ظ').Replace('Ú', 'ع').Replace('Û', 'غ').Replace('Ý', 'ف')
                                     .Replace('Þ', 'ق').Replace('ß', 'ك').Replace('', 'گ').Replace('á', 'ل').Replace('ã', 'م').Replace('ä', 'ن').Replace('æ', 'و');*/

                                    //checkIfIt'sPersianNumber
                                    //bool IsPersianNum = false;
                                    ////var firstLen = t.Length;
                                    //if (t.Length > 0)
                                    //{
                                    //    string IsNum = t.Replace("پ", "").Replace("€", "").Replace("‚", "").Replace("ƒ", "").
                                    //        Replace("„", "").Replace("…", "").Replace("†", "").Replace("‡", "").Replace("ˆ", "").Replace("‰", "");
                                    //    if (IsNum.Length == 0) IsPersianNum = true;
                                    //    else if (IsNum == "//") IsPersianNum = true;

                                    //}

                                    //endOfPersianCheck
                                    var IsPersianNum = t.IsPersianNum();
                                    if (IsPersianNum)
                                    {
                                        t = t.RevStr();
                                    }
                              
                                    t = t.Replace("پ", "1").Replace("چ", "آ").Replace("ژ", "ئ").Replace("ڈ", "ء").Replace("گ", "ا").Replace("‘", "ا").Replace("’", "ب ").Replace("“", "ب").
                                            Replace("”", "پ ").Replace("•", "پ").Replace("–", "ت ").Replace("—", "ت").Replace("ک", "ث ").Replace("™", "ث").Replace("ڑ", "ج ").
                                            Replace("›", "ج").Replace("œ", "چ ").Replace("‌", "چ").Replace("‍", "ح ").Replace("ں", "ح").Replace(" ", "خ ").Replace("،", "خ").
                                            Replace("¢", "د").Replace("£", "ذ").Replace("¤", "ر").Replace("¥", "ز").Replace("¦", "ژ").Replace("§", "س ").Replace("¨", "س").
                                            Replace("©", "ش ").Replace("ھ", "ش").Replace("«", "ص ").Replace("¬", "ص").Replace("­", "ض ").Replace("®", "ض").Replace("¯", "ط").
                                            Replace("à", "ظ").Replace("ل", "ع ").Replace("â", "ع ").Replace("م", "ع").Replace("ن", "ع").Replace("ه", "غ ").Replace("و", "غ ").
                                            Replace("ç", "غ").Replace("è", "غ").Replace("é", "ف ").Replace("ê", "ف").Replace("ë", "ق ").Replace("ى", "ق").Replace("ي", "ك ").
                                            Replace("î", "ك").Replace("ï", "گ ").Replace("ً", "گ").Replace("ٌ", "ل ").Replace("ٍ", "لا").Replace("َ", "ل").Replace("ô", "م ").
                                            Replace("ُ", "م").Replace("ِ", "ن ").Replace("÷", "ن").Replace("ّ", "و").Replace("ù", "ه ").Replace("ْ", "ه").Replace("û", "ه").
                                            Replace("ü", "? ").Replace("‎", "ي ").Replace("‏", "ي").Replace("€", "0").Replace("‚", "2").Replace("ƒ", "3").
                                            Replace("„", "4").Replace("…", "5").Replace("†", "6").Replace("‡", "7").Replace("ˆ", "8").Replace("‰", "9").Replace("x", "x").
                                            /*Replace("(", ")").Replace(")", "(").*/Replace("-", "-").Replace("_", "_").Replace("‹", "-");
                                    t = t.Replace(Convert.ToChar(63), 'ی');

                                    if (!IsPersianNum && t.ContainsPersianNum())
                                    {
                                        var mc = System.Text.RegularExpressions.Regex.Matches(t, @"[0-9]+");
                                        foreach (var m in mc)
                                        {
                                            string ms = ((System.Text.RegularExpressions.Match)m).Value;
                                 
                                            if(ms.Last() == '-' && ms.Length > 1)
                                            {
                                                try
                                                {
                                                    var indexOfms = t.IndexOf(ms);
                                                    ms = t.Substring(indexOfms, ms.Length-1);
                                                }
                                                catch { }

                                            }
                                            t = t.Replace(ms, ms.RevStr());
                                        }
                                        //t = new String(RevArray);
                                    }



                                    string ret = "";
                                    for (int j = 0; j < t.Length; j++)
                                    {
                                        if (t[j] == '(') ret += ')';
                                        else if (t[j] == ')') ret += '(';
                                        else ret += t[j];
                                    }
                                    row[fieldIndex] = ret;

                                }
                                else
                                {
                                    var byteArr = recReader.ReadBytes(field.fieldLen);
                                    for (int i = 0; i < byteArr.Length; i++)
                                    {
                                        if (byteArr[i] > 127)
                                        {
                                            byteArr[i] = hrhSuggestedCharMap[byteArr[i] - 128];
                                        }
                                    }
                                    var strVal4 = Encoding.Default.GetString(byteArr);


                                    char[] arrayRev = strVal4.ToCharArray();
                                    Array.Reverse(arrayRev);
                                    var tt = new String(arrayRev);

                                    row[fieldIndex] = tt;
                                }
                                break;

                            case 'D': // Date (YYYYMMDD)
                                year = readingEncoding.GetString(recReader.ReadBytes(4));
                                month = readingEncoding.GetString(recReader.ReadBytes(2));
                                day = readingEncoding.GetString(recReader.ReadBytes(2));
                                row[fieldIndex] = System.DBNull.Value;
                                try
                                {
                                    if (IsNumber(year) && IsNumber(month) && IsNumber(day))
                                    {
                                        if ((Int32.Parse(year) > 1900))
                                        {
                                            row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                                        }
                                    }
                                }
                                catch
                                { }

                                break;

                            case 'T':
                                lDate = recReader.ReadInt32();
                                lTime = recReader.ReadInt32() * 10000L;
                                row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
                                break;

                            case 'L': // Boolean (Y/N)
                                if ('Y' == recReader.ReadByte())
                                {
                                    row[fieldIndex] = true;
                                }
                                else
                                {
                                    row[fieldIndex] = false;
                                }

                                break;

                            case 'F':
                                number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                if (IsNumber(number))
                                {
                                    row[fieldIndex] = double.Parse(number);
                                }
                                else
                                {
                                    row[fieldIndex] = 0.0F;
                                }
                                break;
                        }
                        fieldIndex++;
                    }
 
                    recReader.Close();
                    dt.Rows.Add(row);
                }
            }

            catch
            {
                throw;
            }
            finally
            {
                if (null != br)
                {
                    br.Close();
                }
            }

            long count = DateTime.Now.Ticks - start;

            //if (bad) MessageBox.Show("ehtemal dadeye gomshode");

            return dt;
        }

        public static bool IsNumber(string numberString)
        {
            char[] numbers = numberString.ToCharArray();
            int number_count = 0;
            int point_count = 0;
            int space_count = 0;

            foreach (char number in numbers)
            {
                if ((number >= 48 && number <= 57))
                {
                    number_count += 1;
                }
                else if (number == 46)
                {
                    point_count += 1;
                }
                else if (number == 32)
                {
                    space_count += 1;
                }
                else
                {
                    //return false;
                }
            }

            return (number_count > 0 && point_count < 2);
        }

        private static DateTime JulianToDateTime(long lJDN)
        {
            double p = Convert.ToDouble(lJDN);
            double s1 = p + 68569;
            double n = Math.Floor(4 * s1 / 146097);
            double s2 = s1 - Math.Floor((146097 * n + 3) / 4);
            double i = Math.Floor(4000 * (s2 + 1) / 1461001);
            double s3 = s2 - Math.Floor(1461 * i / 4) + 31;
            double q = Math.Floor(80 * s3 / 2447);
            double d = s3 - Math.Floor(2447 * q / 80);
            double s4 = Math.Floor(q / 11);
            double m = q + 2 - 12 * s4;
            double j = 100 * (n - 49) + i + s4;
            return new DateTime(Convert.ToInt32(j), Convert.ToInt32(m), Convert.ToInt32(d));
        }


    }
}
