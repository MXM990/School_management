using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    internal class SqlExec
    {
        static string AddParametraInProc(string prc , List<string> parm)
        {
            string SqlCommandString;

            SqlCommandString = @"Exec " + prc +" ";

            for (int i = 0; i < parm.Count; i++)
            {
                SqlCommandString += " @Value" + i;
                if (i < parm.Count - 1)
                {
                    SqlCommandString += " , ";
                }
            }
            return SqlCommandString;
        }

        public static bool Exec_proc(string FinalPrc,List<string> parm)
        {
            if (CurrentConnection.OpenConntion())
            {
                SqlCommand sqlcmd = new SqlCommand(FinalPrc, CurrentConnection.CuCon);
                for (int i = 0; i < parm.Count; i++)
                {
                    sqlcmd.Parameters.AddWithValue("@Value" + i, parm[i]);
                }
                sqlcmd.ExecuteNonQuery();
                CurrentConnection.CloseConntion();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool AddAndExecPrc(string prc , List<string> parm)
        {
            string F_prc = AddParametraInProc(prc, parm);
            if (Exec_proc(F_prc, parm))
            {
                return true;

            }
            return false;
        }
    }
}
