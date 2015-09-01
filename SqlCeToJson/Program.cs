using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Greymind.Tools.SqlCeToJson
{
    class Program
    {
        public static readonly string DatabasesFolder = Path.GetFullPath(@"..\..\..\Databases\");
        public static readonly string DatabaseFilename = "WeWantTrubel";

        public static readonly string SdfFilePath = $@"{DatabasesFolder}\Sdf\{DatabaseFilename}.sdf";
        public static readonly string JsonFilePath = $@"{DatabasesFolder}\Json\{DatabaseFilename}.json";

        static Program()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(JsonFilePath));
        }

        static void Main(string[] args)
        {
            var connectionString = $"Data Source = \"{SdfFilePath}\"";

            SqlCeConnection connection = null;
            SqlCeCommand command = null;
            SqlCeDataReader reader = null;

            try
            {
                connection = new SqlCeConnection(connectionString);
                connection.Open();

                var query = "SELECT * FROM Petitioners";
                command = new SqlCeCommand(query, connection);
                reader = command.ExecuteReader();

                var json = new List<ExpandoObject>();

                while (reader.Read())
                {
                    var item = new ExpandoObject();

                    Enumerable.Range(0, reader.FieldCount)
                        .Select(i => new { Field = reader.GetName(i), Value = reader[i] })
                        .ToList()
                        .ForEach(data => ((IDictionary<string, Object>)item)[data.Field] = data.Value);

                    json.Add(item);
                }

                using (var jsonWriter = new StreamWriter($"{JsonFilePath}", false, Encoding.Default))
                {
                    jsonWriter.WriteLine(JsonConvert.SerializeObject(json, Formatting.Indented));
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                reader?.Close();
                command?.Dispose();
                connection?.Close();
            }

            Console.WriteLine($"{Environment.NewLine}Press any key to exit.");
            Console.ReadKey();
        }
    }
}
