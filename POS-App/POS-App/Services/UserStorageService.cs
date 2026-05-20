using POS_App.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace POS_App.Services
{
    public static class UserStorageService
    {
        private static readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "POSApp", "user.json");

        public static void Save(UserDto user)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(user,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        public static UserDto Load()
        {
            if (!File.Exists(FilePath)) return null;
            try
            {
                return JsonSerializer.Deserialize<UserDto>(File.ReadAllText(FilePath));
            }
            catch
            {
                return null; // File bị corrupt → coi như chưa login
            }
        }

        public static void Clear()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
    }
}
