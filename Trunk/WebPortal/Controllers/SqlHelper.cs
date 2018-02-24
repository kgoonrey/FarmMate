using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Controllers
{
    public class SqlHelper
    {
        public static DataTable RunQuery(string query, Dictionary<string, object> parameters)
        {
            var conn = new SqlConnection("Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
            System.Data.DataTable dt = new DataTable();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(query, conn);

            foreach (var item in parameters)
                da.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);

            da.Fill(dt);
            return dt;
        }

        public static void RunScalarQuery(string query, Dictionary<string, object> parameters)
        {
            var conn = new SqlConnection("Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
            System.Data.DataTable dt = new DataTable();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(query, conn);

            foreach (var item in parameters)
                da.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);

            da.Update(dt);
        }
    }
}
