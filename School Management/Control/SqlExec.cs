using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    internal class SqlExec
    {


        public static bool Exec_proc(string procName, List<SqlParameter> parameters)
        {
            if (CurrentConnection.OpenConntion())
            {
                try
                {
                    using (SqlCommand sqlcmd = new SqlCommand(procName, CurrentConnection.CuCon))
                    {
                        sqlcmd.CommandType = CommandType.StoredProcedure;  
                        sqlcmd.Parameters.AddRange(parameters.ToArray());
                        sqlcmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    CurrentConnection.CloseConntion();
                }
            }
            else
            {
                return false;
            }
        }
        public static int GetRecordCount(string query)
        {
            try
            {
                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        var result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }
    }
}
