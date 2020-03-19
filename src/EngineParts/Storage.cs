using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitEngineDB.EngineParts
{
    internal class Storage
    {
        public string _dbBasePath;
        public string _dbUser;
        public string _dbUserEmail;

        public Storage(string dbBasePath, string user, string userEmail)
        {
            _dbBasePath = dbBasePath;
            _dbUser = user;
            _dbUserEmail = userEmail;
        }

        public ConcurrentDictionary<string, string> CreateCache()
        {
            var root = string.Empty;
            if (!Directory.Exists(_dbBasePath))
            {
                Directory.CreateDirectory(_dbBasePath);
            }
            else
            {
                root = Repository.Discover(_dbBasePath);
            }

            if(string.IsNullOrEmpty(root))
            {
                Repository.Init(_dbBasePath);
                // This is a very strange quirk when calling Init
                // A link file (_git_<random hex>) is created to a none existant testing folder
                // I think that this will be fixed in future versions of LibGit2Sharp
                // In mean time, since I expect an empty folder after the Init 
                // I will just delete everything except .git folder
                foreach (var dir in Directory.GetDirectories(_dbBasePath)
                                             .Where(d => d != Path.Combine(_dbBasePath, ".git")))
                {
                    Directory.Delete(dir);
                }
            }

            else if(root != Path.Combine(Path.GetFullPath(_dbBasePath), ".git\\"))
            {
                throw new InvalidOperationException($"Directory {_dbBasePath} is a sub directory of an existing repository, unable to create a repository");
            }            
            
            var dbCache = new ConcurrentDictionary<string, string>();
            using (var repo = new Repository(_dbBasePath))
            {
                var status = repo.RetrieveStatus();                
                if(status.IsDirty)
                {
                    CommitChanges();
                }
                foreach(var item in status)
                {                    
                    dbCache.TryAdd(item.FilePath, File.ReadAllText(Path.Combine(_dbBasePath, item.FilePath)));
                }                
            }            
            return dbCache;
        }

        public void UpdateData(string fileName, string data)
        {
            lock (_dbBasePath)
            {
                File.WriteAllText(Path.Combine(_dbBasePath, fileName), data);
            }
        }
        public void CommitChanges(string message = "Data updated")
        {
            using (var repo = new Repository(_dbBasePath))
            {
                Commands.Stage(repo, "*");
                repo.Index.Write();

                var sig = new Signature(_dbUser, _dbUserEmail, DateTime.Now);
                lock (_dbBasePath)
                {
                    repo.Commit(message, sig, sig);
                }
            }
        }
    }
}