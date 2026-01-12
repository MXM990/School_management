using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    public class CreatetableProc
    {
        public static bool CreateAllTable(SqlConnection connection)
        {
            foreach (string procedureCommand in CreateTableString.CreateTables)
            {
                using (SqlCommand command = new SqlCommand(procedureCommand, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool CreateAllProcedures(SqlConnection connection)
        {
            foreach (string procedureCommand in ProcInsertString.CreateProceduresCommands)
            {
                using (SqlCommand command = new SqlCommand(procedureCommand, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
