using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greymind.Tools.SqlCeToCsv
{
    class Program
    {
        public static readonly string DatabasesFolder = Path.GetFullPath(@"..\..\..\Databases\");
        public static readonly string DatabaseFilename = "WeWantTrubel";

        public static readonly string SdfFilePath = $@"{DatabasesFolder}\Sdf\{DatabaseFilename}.sdf";
        public static readonly string CsvFilePath = $@"{DatabasesFolder}\Csv\{DatabaseFilename}.csv";

        static Program()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CsvFilePath));
        }

        static void Main(string[] args)
        {
            var connectionString = $"Data Source = \"{SdfFilePath}\"";

            SqlCeConnection connection = null;
            SqlCeCommand command = null;
            SqlCeDataReader reader = null;
            StreamWriter csvWriter = null;

            try
            {
                connection = new SqlCeConnection(connectionString);
                connection.Open();

                var query = "SELECT * FROM Petitioners";
                command = new SqlCeCommand(query, connection);
                reader = command.ExecuteReader();

                csvWriter = new StreamWriter($"{CsvFilePath}", false, Encoding.Default);

                csvWriter.WriteLine(Enumerable.Range(0, reader.FieldCount)
                    .Select(i => $"\"{reader.GetName(i)}\"")
                    .Aggregate((current, next) => $"{current},{next}"));

                while (reader.Read())
                {
                    csvWriter.WriteLine(Enumerable.Range(0, reader.FieldCount)
                        .Select(i => String.Format("\"{0}\"",
                            reader.GetValue(i)
                                .ToString()
                                .Replace(',', '_')
                                .Replace('\"', '_')))
                        .Aggregate((current, next) => $"{current},{next}"));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                csvWriter?.Close();
                reader?.Close();
                command?.Dispose();
                connection?.Close();
            }

            Console.WriteLine($"{Environment.NewLine}Press any key to exit.");
            Console.ReadKey();
        }
    }
}
