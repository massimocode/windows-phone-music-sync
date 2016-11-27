using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WindowsPhoneMusicSync
{
    public class PhoneRepository
    {
        public static async Task<PhoneLibraryIndex> GetPhoneLibraryIndex(string phoneName)
        {
            var index = new PhoneLibraryIndex();

            try
            {
                var musicFolder = await GetPhoneMusicFolder(phoneName);
                var library = await musicFolder.GetFileAsync("Library.xml").AsTask();
                using (var stream = await library.OpenReadAsync().AsTask())
                using (var dataReader = new DataReader(stream))
                {
                    var streamLength = (uint)stream.Size;
                    await dataReader.LoadAsync(streamLength).AsTask();
                    var contents = dataReader.ReadString(streamLength);
                    var xml = XDocument.Parse(contents);
                    foreach (var file in xml.Descendants("File"))
                    {
                        index.AddOrUpdateFile(new PhoneMusicFile { Path = file.Attribute("Path").Value, Version = file.Attribute("Version").Value });
                    }
                    return index;
                }
            }
            catch
            {
                return index;
            }
        }

        public static async Task SavePhoneLibraryIndex(StorageFolder phoneMusicLibrary, PhoneLibraryIndex phoneLibraryIndex)
        {
            var xml = new XDocument(
                new XElement("Library",
                    phoneLibraryIndex.GetAllFiles().Select(x =>
                    {
                        var element = new XElement("File");
                        element.SetAttributeValue("Path", x.Path);
                        element.SetAttributeValue("Version", x.Version);
                        return element;
                    })
                )
            );

            var xmlFile = await phoneMusicLibrary.CreateFileAsync("Library.xml", CreationCollisionOption.ReplaceExisting).AsTask();

            using (var textStream = await xmlFile.OpenAsync(FileAccessMode.ReadWrite).AsTask())
            using (var dataWriter = new DataWriter(textStream))
            {
                dataWriter.WriteString(xml.ToString());
                await dataWriter.StoreAsync().AsTask();
            }
        }

        public static async Task<StorageFolder> GetPhoneMusicFolder(string phoneName)
        {
            var storage = (await KnownFolders.RemovableDevices.GetFoldersAsync().AsTask()).First(a => a.Name == phoneName);
            return await storage.GetFolderAsync(@"Phone\Music\From PC").AsTask();
        }
    }
}
