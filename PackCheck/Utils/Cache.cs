using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PackCheck.Commands;

namespace PackCheck.Utils
{
    public class Cache
    {
        private const string CacheFolderPath = ".cache";

        public Cache()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
        }

        public async Task<TData?> GetAsync<TData>(string key, Func<Task<TData?>> cb)
        {
            var path = BuildFullPath(key);

            if (File.Exists(path))
            {
                await using FileStream fs = File.OpenRead(path);
                TData? data = await JsonSerializer.DeserializeAsync<TData>(fs);

                return data;
            }

            TData? fetchedData = await cb();
            if (fetchedData is not null)
            {
                await using FileStream fs = File.OpenWrite(path);
                await JsonSerializer.SerializeAsync(fs, fetchedData, typeof(TData));
            }

            return fetchedData;
        }

        private string BuildFullPath(string key)
        {
            var cwd = Directory.GetCurrentDirectory();
            var fileName = $"{key.ToLower()}.json";

            return Path.Combine(cwd, CacheFolderPath, fileName);
        }
    }
}