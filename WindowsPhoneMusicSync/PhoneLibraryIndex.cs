using System.Collections.Generic;
using System.Linq;

namespace WindowsPhoneMusicSync
{
    public class PhoneLibraryIndex
    {
        private readonly IDictionary<string, PhoneMusicFile> _storage;

        public PhoneLibraryIndex()
        {
            _storage = new Dictionary<string, PhoneMusicFile>();
        }

        public void AddOrUpdateFile(PhoneMusicFile file)
        {
            _storage[file.Path] = file;
        }

        public PhoneMusicFile GetFile(string fileName)
        {
            return _storage.ContainsKey(fileName) ? _storage[fileName] : null;
        }

        public IList<PhoneMusicFile> GetAllFiles()
        {
            return _storage.Select(x => x.Value).ToList();
        }
    }

    public class PhoneMusicFile
    {
        public string Path { get; set; }
        public string Version { get; set; }
    }
}