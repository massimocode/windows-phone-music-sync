using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main()
        {
            Console.Write("Enter the name of your phone as it appears (case-sensitive) in My Computer (i.e. Windows phone): ");
            var phoneName = Console.ReadLine();
            MainAsync(phoneName).Wait();
        }

        static async Task MainAsync(string phoneName)
        {
            if (phoneName == "")
            {
                phoneName = "Windows phone";
            }

            var localMusicLibrary = KnownFolders.MusicLibrary;
            var phoneMusicLibrary = await GetPhoneMusicLibrary(phoneName);

            var folders = await localMusicLibrary.GetFoldersAsync().AsTask();

            foreach (var sourceFolder in folders)
            {
                var destinationFolder = await GetOrCreateSubFolder(phoneMusicLibrary, sourceFolder.Name);

                var sourceFiles = await sourceFolder.GetFilesAsync().AsTask();
                foreach (var sourceFile in sourceFiles)
                {
                    var needsSyncing = true;
                    try
                    {
                        var destinationFile = await destinationFolder.GetFileAsync(sourceFile.Name).AsTask();
                        var destinationFileProperties = await destinationFile.GetBasicPropertiesAsync().AsTask();
                        if (destinationFileProperties.Size == (await sourceFile.GetBasicPropertiesAsync().AsTask()).Size)
                        {
                            needsSyncing = false;
                            Console.WriteLine("Skipping " + sourceFile.Path);
                        }
                    }
                    catch
                    {
                    }
                    if (needsSyncing)
                    {
                        Console.WriteLine("Copying " + sourceFile.Path);
                        await sourceFile.CopyAsync(destinationFolder, sourceFile.Name, NameCollisionOption.ReplaceExisting).AsTask();
                        Console.WriteLine("Copied " + sourceFile.Path);
                    }
                }
            }
        }

        private static async Task<IStorageFolder> GetOrCreateSubFolder(IStorageFolder parentFolder, string subFolderName)
        {
            return await parentFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists).AsTask();
        }

        private static async Task<StorageFolder> GetPhoneMusicLibrary(string phoneName)
        {
            var storage = (await KnownFolders.RemovableDevices.GetFoldersAsync().AsTask()).First(a => a.Name == phoneName);
            return await storage.GetFolderAsync(@"Phone\Music\From PC").AsTask();
        }
    }

    public static class Extensions
    {
        public static Task<T> AsTask<T>(this IAsyncOperation<T> op)
        {
            var ret = new TaskCompletionSource<T>();
            op.Completed = (info, status) =>
            {
                if (status == AsyncStatus.Completed)
                {
                    ret.SetResult(info.GetResults());
                }
                if (status == AsyncStatus.Canceled)
                {
                    ret.SetCanceled();
                }
                if (status == AsyncStatus.Error)
                {
                    ret.SetException(info.ErrorCode);
                }
            };
            return ret.Task;
        }
    }
}
