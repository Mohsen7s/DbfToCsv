using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DosToWin
{
    public static class Extensions
    {
        public static bool IsPersianNum(this string Inp)
        {
            if (Inp.Length > 0)
            {
                string IsNum = Inp.Replace("پ", "").Replace("€", "").Replace("‚", "").Replace("ƒ", "").
                    Replace("„", "").Replace("…", "").Replace("†", "").Replace("‡", "").Replace("ˆ", "").Replace("‰", "");

                if (IsNum.Length == 0) return true;
                else if (IsNum == "//") return true;
            }
            return false;
        }
        public static string RevStr(this string Inp)
        {
            char[] RevArray = Inp.ToCharArray();
            Array.Reverse(RevArray);
            return new String(RevArray);
        }

        public static bool ContainsPersianNum(this string Inp)
        {
            foreach (var item in "0123456789")
            {
                if (Inp.Contains(item + "")) return true;
            }
            return false;

            //int bLen = Inp.Length;
            //if (Inp.Length > 0)
            //{
                
                
            //    if (IsNum.Length != bLen) return true;
            //}
            //return false;
        }




    }
}
