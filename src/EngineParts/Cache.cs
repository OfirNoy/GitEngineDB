using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Collections.Concurrent;
using GitEngineDB.Logging;
using LibGit2Sharp;
using System.Threading.Tasks;

namespace GitEngineDB.EngineParts
{
    internal class Cache
    {
        const int DIRTY_BUFFER_COUNT = 2;
        private readonly Timer _timer;
        private readonly Storage _storage;                
        private readonly ConcurrentDictionary<string, string> _state;
        private Dictionary<string, string>[] _dirty = new Dictionary<string, string>[DIRTY_BUFFER_COUNT];
        const int BUFFER_0 = 0;
        const int BUFFER_1 = 1;
        private int ACTIVE_BUFFER = 0;
                
        #region Constructor
        public Cache(string dbBasePath, string user, string userEmail, EngineConfigurationOptions options)
        {
            _storage = new Storage(dbBasePath, user, userEmail);
            _state = _storage.CreateCache();
            
            _timer = new Timer(options.WriteIntervals);
            _timer.Elapsed += CommitChangesHandler;
            _timer.Start();

            _dirty[BUFFER_0] = new Dictionary<string, string>();
            _dirty[BUFFER_1] = new Dictionary<string, string>();
        }
        #endregion

        public Dictionary<string, string> GetData()
        {
            // Return shallow copy of the current state
            return _state.ToDictionary(x => x.Key, x => x.Value);         
        }

        public string GetData(string fileName)
        {
            if(_state.TryGetValue(fileName, out var data))
            {
                return data;
            }
            return string.Empty;
        }

        public void SetData(string fileName, string data)
        {            
            _state.AddOrUpdate(fileName, data, (k, v) => data);
            lock (_dirty[ACTIVE_BUFFER])
            {
                var activeBuffer = _dirty[ACTIVE_BUFFER];
                if (!activeBuffer.ContainsKey(fileName))
                {
                    activeBuffer.Add(fileName, data);
                }
                else
                {
                    activeBuffer[fileName] = data;
                }
            }
        }
        public Task CommitChanges()
        {
            return Task.Factory.StartNew(() => _storage.CommitChanges());
        }
        private void CommitChangesHandler(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                while (_dirty[ACTIVE_BUFFER].Count() > 0)
                {
                    var oldBuffer = _dirty[ACTIVE_BUFFER];
                    lock (oldBuffer)
                    {
                        // Swap to the other buffer
                        ACTIVE_BUFFER = ACTIVE_BUFFER == BUFFER_0 ? BUFFER_1 : BUFFER_0;
                    }

                    foreach (var kp in oldBuffer)
                    {   
                       _storage.SetData(kp.Key, kp.Value);
                    }                    

                    oldBuffer.Clear();
                }
            }
            catch(EmptyCommitException)
            {
                // Do nothing
            }
            catch(Exception ex)
            {
                DbLogger.Error(ex, "Failed to commit changes to disk");
            }
            finally
            {
                _timer.Start();
            }
        }
    }
}