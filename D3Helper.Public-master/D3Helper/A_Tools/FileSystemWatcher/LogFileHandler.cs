using System;
using System.IO;
using System.Runtime.Caching;

namespace D3Helper.A_Tools.FileSystemWatcherMemoryCache
{
    /// <summary>
    /// AddOrGetExisting a file event to MemoryCache, to block/swallow multiple events
    /// Actually 'handle' event inside callback for removal from cache on event expiring
    /// </summary>
    internal class LogFileHandler
    {
        private readonly MemoryCache _memCache;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private const double CacheTimeMilliseconds = 50;

        public string Path
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RoS-BoT\\Logs";
            }
        }

        // Setup a FileSystemWatcher and cache item policy shared settings
        public LogFileHandler()
        {
            _memCache = MemoryCache.Default;

            var watcher = new FileSystemWatcher()
            {
                Path = Path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "logs.txt"
            };

            _cacheItemPolicy = new CacheItemPolicy()
            {
                RemovedCallback = OnRemovedFromCache
            };

            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Watching for writes to text files in folder: {Path}");
        }

        // Handle cache item expiring 
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

            var e = (FileSystemEventArgs)args.CacheItem.Value;

            Console.WriteLine($"Let's now respond to the event {e.ChangeType} on {e.FullPath}");
        }

        // Add file event to cache (won't add if already there so assured of only one occurance)
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);
            _memCache.AddOrGetExisting(e.Name, e, _cacheItemPolicy);
        }
    }
}