using GitEngineDB.EngineParts;
using GitEngineDB.Logging;
using System.Text.Json;

namespace GitEngineDB.Engine
{
    public class GitDbEngine
    {
        private Cache _cache;

        public GitDbEngine(string dbBasePath, string user, string userEmail, EngineConfigurationOptions options = null)
        {
            DbLogger.Init(options?.Logger);
            _cache = new Cache(dbBasePath, user, userEmail, options ?? new EngineConfigurationOptions());
        }
        public T GetData<T>(string dataId)
        {
            return JsonSerializer.Deserialize<T>(_cache.GetData($"GEDB.{dataId}"));
        }

        public string GetRawData(string dataId)
        {
            return _cache.GetData($"GEDB.{dataId}");
        }

        public void SetData<T>(string dataId, T data)
        {
            _cache.SetData($"GEDB.{dataId}", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void CommitChanges()
        {
            _cache.CommitChanges();
        }
    }
}
