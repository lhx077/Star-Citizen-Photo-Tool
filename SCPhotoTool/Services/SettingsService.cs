using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private SettingsData _settings;

        public SettingsService()
        {
            // 设置文件存储在用户本地应用数据目录
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SCPhotoTool");
                
            // 确保目录存在
            Directory.CreateDirectory(appDataPath);
            
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            // 加载设置
            LoadSettings();
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (_settings.Values.TryGetValue(key, out object value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        // 处理JsonElement的类型转换
                        if (typeof(T) == typeof(int) && jsonElement.ValueKind == JsonValueKind.Number)
                            return (T)(object)jsonElement.GetInt32();
                        else if (typeof(T) == typeof(string) && jsonElement.ValueKind == JsonValueKind.String)
                            return (T)(object)jsonElement.GetString();
                        else if (typeof(T) == typeof(bool) && jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                            return (T)(object)jsonElement.GetBoolean();
                        else
                            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions);
                    }
                    
                    return (T)value;
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }

        public async Task<bool> SaveSettingAsync<T>(string key, T value)
        {
            try
            {
                _settings.Values[key] = value;
                return await SaveSettingsAsync();
            }
            catch
            {
                return false;
            }
        }

        public string GetScreenshotDirectory()
        {
            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "Star Citizen Photos");
                
            return GetSetting<string>("ScreenshotDir", defaultPath);
        }

        public async Task<bool> SetScreenshotDirectoryAsync(string path)
        {
            return await SaveSettingAsync("ScreenshotDir", path);
        }

        public string GetScreenshotFileNameFormat()
        {
            return GetSetting<string>("FileNameFormat", "SC_Photo_{0:yyyyMMdd_HHmmss}");
        }

        public async Task<bool> SetScreenshotFileNameFormatAsync(string format)
        {
            return await SaveSettingAsync("FileNameFormat", format);
        }

        public int GetScreenshotQuality()
        {
            return GetSetting<int>("ScreenshotQuality", 90);
        }

        public async Task<bool> SetScreenshotQualityAsync(int quality)
        {
            // 确保质量值在有效范围内
            if (quality < 0) quality = 0;
            if (quality > 100) quality = 100;
            
            return await SaveSettingAsync("ScreenshotQuality", quality);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json, _jsonOptions);
                }
            }
            catch
            {
                // 如果加载失败，创建新的设置对象
            }
            
            // 如果设置为null或加载失败，初始化一个新的
            if (_settings == null)
            {
                _settings = new SettingsData();
            }
        }

        private async Task<bool> SaveSettingsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, _jsonOptions);
                await File.WriteAllTextAsync(_settingsFilePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private class SettingsData
        {
            public System.Collections.Generic.Dictionary<string, object> Values { get; set; } = 
                new System.Collections.Generic.Dictionary<string, object>();
        }
    }
} 