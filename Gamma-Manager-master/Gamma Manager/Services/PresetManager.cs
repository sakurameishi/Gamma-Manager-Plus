using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Gamma_Manager.Models;

namespace Gamma_Manager.Services
{
    public class PresetManager
    {
        private string _filePath;
        private List<Preset> _presets;
        private JavaScriptSerializer _serializer;

        public PresetManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "GammaManager");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _filePath = Path.Combine(appFolder, "presets.json");
            _presets = new List<Preset>();
            _serializer = new JavaScriptSerializer();
            LoadPresets();
        }

        public void LoadPresets()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    _presets = _serializer.Deserialize<List<Preset>>(json);
                    if (_presets == null) _presets = new List<Preset>();
                }
                catch
                {
                    _presets = new List<Preset>();
                }
            }
        }

        public void SavePresets()
        {
            string json = _serializer.Serialize(_presets);
            File.WriteAllText(_filePath, json);
        }

        public List<Preset> GetAllPresets()
        {
            return _presets;
        }

        public void AddPreset(Preset preset)
        {
            _presets.Add(preset);
            SavePresets();
        }

        public void UpdatePreset(Preset preset)
        {
            var existing = _presets.FirstOrDefault(p => p.Id == preset.Id);
            if (existing != null)
            {
                _presets.Remove(existing);
            }
            _presets.Add(preset);
            SavePresets();
        }

        public void DeletePreset(string id)
        {
            var existing = _presets.FirstOrDefault(p => p.Id == id);
            if (existing != null)
            {
                _presets.Remove(existing);
                SavePresets();
            }
        }

        public Preset GetPreset(string id)
        {
            return _presets.FirstOrDefault(p => p.Id == id);
        }
    }
}
