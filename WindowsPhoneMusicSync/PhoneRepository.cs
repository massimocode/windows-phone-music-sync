using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using TestStack.White;
using TestStack.White.InputDevices;
using TestStack.White.WindowsAPI;

namespace WindowsPhoneMusicSync
{
    public class PhoneRepository
    {
        private readonly string _phoneName;

        public PhoneRepository(string phoneName)
        {
            _phoneName = phoneName;
        }

        public async Task<PhoneLibraryIndex> GetPhoneLibraryIndex()
        {
            var index = new PhoneLibraryIndex();

            try
            {
                var musicFolder = await GetPhoneMusicFolder();
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

        public async Task SavePhoneLibraryIndex(PhoneLibraryIndex phoneLibraryIndex)
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

            var musicFolder = await GetPhoneMusicFolder();
            var xmlFile = await musicFolder.CreateFileAsync("Library.xml", CreationCollisionOption.ReplaceExisting).AsTask();

            using (var textStream = await xmlFile.OpenAsync(FileAccessMode.ReadWrite).AsTask())
            using (var dataWriter = new DataWriter(textStream))
            {
                dataWriter.WriteString(xml.ToString());
                await dataWriter.StoreAsync().AsTask();
            }
        }

        public Task<StorageFolder> GetPhoneMusicFolder()
        {
            return GetFolder("Music");
        }

        public async Task<StorageFolder> CreateFolder(string pathRelativeToRoot)
        {
            var folder = await GetFolder();
            var currentPath = "";
            foreach (var directory in pathRelativeToRoot.Split('\\'))
            {
                Console.WriteLine("Creating " + directory + " inside " + folder.Name);
                var subFolder = await folder.TryGetItemAsync(directory).AsTask() as StorageFolder;
                if (subFolder == null)
                {
                    await CreateFolderThroughExplorer(currentPath, directory);
                    subFolder = await folder.GetFolderAsync(directory).AsTask();
                }
                folder = subFolder;
                currentPath += directory + "\\";
            }

            return folder;
        }

        public async Task<StorageFolder> GetFolder(string pathRelativeToRoot = null)
        {
            var storage = (await KnownFolders.RemovableDevices.GetFoldersAsync().AsTask()).First(a => a.Name == _phoneName);

            if (pathRelativeToRoot == null)
            {
                return await storage.GetFolderAsync("Phone").AsTask();
            }

            return await storage.TryGetItemAsync($"Phone\\{pathRelativeToRoot}").AsTask() as StorageFolder;
        }

        public async Task DeleteFolder(string pathRelativeToRoot)
        {
            var folder = await GetFolder(pathRelativeToRoot);
            await folder.DeleteAsync().AsTask();
        }

        private async Task CreateFolderThroughExplorer(string parentFolder, string folderName)
        {
            const int delay = 200;

            // Open explorer window
            Application.Launch("explorer");
            var windows = Desktop.Instance.Windows();
            var window = windows.First(x => string.IsNullOrEmpty(x.Title));
            await Task.Delay(delay);

            // Focus the navigation bar
            Keyboard.Instance.HoldKey(KeyboardInput.SpecialKeys.CONTROL);
            Keyboard.Instance.Send("l", window);
            Keyboard.Instance.LeaveAllKeys();
            await Task.Delay(delay);

            // Navigate to the parent folder
            Keyboard.Instance.Send("This PC\\" + _phoneName + "\\Phone\\" + parentFolder, window);
            await Task.Delay(delay);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);
            Keyboard.Instance.LeaveAllKeys();
            await Task.Delay(delay * 5);

            // Create new folder
            Keyboard.Instance.HoldKey(KeyboardInput.SpecialKeys.SHIFT);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.F10);
            Keyboard.Instance.LeaveAllKeys();
            Keyboard.Instance.Send("n", window);
            await Task.Delay(delay);

            // Enter folder name
            Keyboard.Instance.Send(folderName, window);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);
            await Task.Delay(delay);

            // Close explorer window
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.LEFT_ALT);
            Keyboard.Instance.Send("FC", window);
            await Task.Delay(delay * 3);
        }
    }
}
