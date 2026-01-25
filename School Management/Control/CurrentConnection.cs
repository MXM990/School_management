using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    internal class CurrentConnection
    {
        public  static SqlConnection CuCon;
        public static bool StringConntionIsNotNull()
        {
            if (CuCon == null)
            {
                return false;
            }
            return true;
        }
        public static bool OpenConntion()
        {
            if (StringConntionIsNotNull())
            {
                try
                {
                    CuCon.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static void CloseConntion()
        {
            CuCon.Close();
        }
    }
}
