using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace Ngsoft.IO
{
    public class FileStorage<T>
        where T : class
    {
        protected static AutoResetEvent WaitHandler { get; } = new AutoResetEvent(true);
        protected string BaseDirectoryPath { get; }
        protected string FilePath { get; }

        public FileStorage(string baseDirectoryPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(baseDirectoryPath))
            {
                throw new ArgumentException("Base directory path cannot be empty.", nameof(baseDirectoryPath));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));
            }

            BaseDirectoryPath = baseDirectoryPath;
            FilePath = Path.Combine(BaseDirectoryPath, fileName);
        }

        public virtual bool Exists()
        {
            WaitHandler.WaitOne();
            var exists = File.Exists(FilePath);
            WaitHandler.Set();
            return exists;
        }

        public virtual T Get()
        {
            WaitHandler.WaitOne();
            if (File.Exists(FilePath) == false)
            {
                WaitHandler.Set();
                return null;
            }
            try
            {
                using (var reader = File.OpenText(FilePath))
                {
                    var content = reader.ReadToEnd();
                    WaitHandler.Set();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch
            {
                WaitHandler.Set();
                throw;
            }
        }

        public virtual void Save(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            WaitHandler.WaitOne();
            try
            {
                // Ensure base directory exists.
                if (Directory.Exists(BaseDirectoryPath) == false)
                {
                    Directory.CreateDirectory(BaseDirectoryPath);
                }

                var content = JsonConvert.SerializeObject(entity, Formatting.Indented);
                using (var writer = File.CreateText(FilePath))
                {
                    writer.WriteLine(content);
                }
                WaitHandler.Set();
            }
            catch
            {
                WaitHandler.Set();
                throw;
            }
        }

        public virtual void Delete()
        {
            WaitHandler.WaitOne();
            if (File.Exists(FilePath) == false)
            {
                WaitHandler.Set();
                return;
            }
            try
            {
                File.Delete(FilePath);
                WaitHandler.Set();
            }
            catch
            {
                WaitHandler.Set();
                throw;
            }
        }
    }
}
