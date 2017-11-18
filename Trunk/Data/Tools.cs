using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace Data
{
    public static class Tools
    {
        /// <summary>
        /// Runs the given query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataTable RunQuery(string query)
        {
            return RunQuery(query, new Dictionary<string, object>());
        }

        /// <summary>
        /// Runs the given query using the supplied parameters
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable RunQuery(string query, Dictionary<string, object> parameters)
        {
            return RunQuery(query, parameters, "Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
        }

        /// <summary>
        /// Runs the given query and fills the given table with the results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters">Can be null</param>
        /// <param name="fillTable"></param>
        public static void RunQueryIntoFillTable(string query, Dictionary<string, object> parameters, DataTable fillTable)
        {
            RunQuery(query, parameters, "Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
        }

        /// <summary>
        /// Runs the given the query using the supplied parameters and connection string
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters">Can be null</param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DataTable RunQuery(string query, Dictionary<string, object> parameters, string connectionString)
        {
            DataTable data = new DataTable();
            RunQuery(query, parameters, connectionString, data);

            return data;
        }


        private static void RunQuery(string query, Dictionary<string, object> parameters, string connectionString, DataTable fillTable)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                    cmd.CommandTimeout = 300000; // 5 mins

                    // Add the parameters to the command
                    if (parameters != null)
                    {
                        foreach (var kvp in parameters)
                            cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);

                    }

                    // Execute and load the data
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        fillTable.Load(reader);
                    }
                }
            }
        }

    }
}
