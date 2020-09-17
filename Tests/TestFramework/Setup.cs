using System;
using System.IO;

namespace TestFramework
{
    public class Setup
    {
        public static Uri D365CeHttpClientBaseAddress => new Uri("http://host.local/demo/api/data/v8.2/");

        public static string GetSetupJsonContent(string fileName)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonContent", fileName);
            var file = new FileInfo(filePath);

            if (!file.Exists)
            {
                throw new FileNotFoundException($"File with name '{filePath}' not found.");
            }

            using var stream = file.OpenRead();
            using var reader = new StreamReader(stream);
            var fileContent = reader.ReadToEnd();

            reader.Close();
            stream.Close();

            return fileContent;
        }

        public const string EntityIdStr = "00000000-0000-0000-0000-000000000001";

        public static Guid EntityId => new Guid(EntityIdStr);
    }
}