using System;
using System.IO;

namespace CrmNx.Crm.Toolkit.Testing
{
    public class SetupBase
    {
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