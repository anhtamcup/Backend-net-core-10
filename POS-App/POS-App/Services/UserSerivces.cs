using POS_App.Models;
using System.IO;
using System.Text.Json;

namespace POS_App.Services
{
    public class UserSerivces
    {
        private static readonly string FilePath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "POSAPP", "user.json");

        public static void Save(UserInfo user)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(user, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(FilePath, json);
        }

        public static UserInfo Load()
        {
            if (!File.Exists(FilePath)) return null;
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<UserInfo>(json);
        }

        public static void Clear()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
    }
}
