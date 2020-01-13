using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Stratus
{
    class Program
    {
        static void Main(string[] args)
        {

            var task = GetAllProject(new Uri("https://api.gtpstratus.com/v1/project/get-all"));
            task.Wait();

            //JArray j = JArray.Parse(task.Result);

            Project[] projects = null;

            JToken token = JToken.Parse(task.Result);
            if (token is JArray)
            {
                projects = token.ToObject<Project[]>();
            }
            else
            {
                Console.WriteLine("It is not an array...");
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("NAME");


            foreach (Project project in projects)
            {
                Console.WriteLine("Poject : - " + project.id);
                DataRow dr = dt.NewRow();
                dr["ID"] = project.id;
                dr["NAME"] = project.name;

                dt.Rows.Add(dr);
            }

            SaveProjects(dt);
            Console.ReadLine();
        }

        private static void SaveProjects(DataTable dt)
        {
            try
            {
                //private readonly string connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"].ToString();
                string data_source = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = IA-ORA-WALD)(PORT = 1521))" +
                                                       "(CONNECT_DATA =(SERVICE_NAME = WALDTEST.waldinger.com))" +
                                                    ");";
                string connectionString = data_source + "User Id=username;Password=password;";
                using (var connection = new OracleConnection(connectionString))
                {
                    int count = dt.Rows.Count;
                    connection.Open();
                    string[] ids = new string[count];
                    string[] names = new string[count];

                    for (int j = 0; j < count; j++)
                    {
                        ids[j] = Convert.ToString(dt.Rows[j]["ID"]);
                        names[j] = Convert.ToString(dt.Rows[j]["NAME"]);
                    }

                    OracleParameter id = new OracleParameter();
                    id.OracleDbType = OracleDbType.Varchar2;
                    id.Value = ids;

                    OracleParameter name = new OracleParameter();
                    name.OracleDbType = OracleDbType.Varchar2;
                    name.Value = names;

                    OracleCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "INSERT INTO PROJECT (ID, NAME) VALUES (:1, :2)";
                    cmd.ArrayBindCount = ids.Length;
                    cmd.Parameters.Add(id);
                    cmd.Parameters.Add(name);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static async Task<string> GetAllProject(Uri uri)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("app-Key", "712c51f5-e73a-d305-88ed-4b22a925ab1a");
                client.DefaultRequestHeaders.Add("accept", "application/json");
                HttpResponseMessage result = await client.GetAsync(uri);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }
    }
}
