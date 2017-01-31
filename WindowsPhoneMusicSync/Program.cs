using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace WindowsPhoneMusicSync
{
    public class Program
    {
        public static void Main()
        {
            Console.Write("Enter the name of your phone as it appears (case-sensitive) in My Computer (i.e. Windows phone): ");
            var phoneName = Console.ReadLine();

            var cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) =>
            {
                cancellationTokenSource.Cancel();
                args.Cancel = true;
            };

            MainAsync(phoneName, cancellationTokenSource.Token).Wait();
        }

        public static async Task MainAsync(string phoneName, CancellationToken cancellationToken)
        {
            if (phoneName == "")
            {
                phoneName = "Windows phone";
            }
            var phoneRepository = new PhoneRepository(phoneName);

            var phoneMusicFolder = await phoneRepository.GetPhoneMusicFolder();
            var phoneLibraryIndex = await phoneRepository.GetPhoneLibraryIndex();

            var localMusicFolder = KnownFolders.MusicLibrary;
            var sourceFolders = await localMusicFolder.GetFoldersAsync().AsTask();
            var destinationFolders = await phoneMusicFolder.GetFoldersAsync().AsTask();

            foreach (var sourceFolder in sourceFolders)
            {
                var destinationFolder = destinationFolders.FirstOrDefault(x => x.Name == sourceFolder.Name);

                if (destinationFolder == null) {
                    Console.WriteLine("Creating folder " + sourceFolder.Name);
                    destinationFolder = await phoneMusicFolder.CreateFolderAsync(sourceFolder.Name, CreationCollisionOption.OpenIfExists).AsTask();
                    Console.WriteLine("Created folder " + sourceFolder.Name);
                }

                var filesInDestinationFolder = await destinationFolder.GetFilesAsync().AsTask();

                var sourceFiles = await sourceFolder.GetFilesAsync().AsTask();
                foreach (var sourceFile in sourceFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var sourceVersion = GetVersion(sourceFile.Path);
                    var path = Path.Combine(sourceFolder.Path, sourceFile.Path);
                    
                    // If the file does not exist or the version is not the same then the file needs syncing
                    var needsSyncing = filesInDestinationFolder.All(x => x.Name != sourceFile.Name) || phoneLibraryIndex.GetFile(path)?.Version != sourceVersion;

                    if (!needsSyncing)
                    {
                        Console.WriteLine("Skipped " + sourceFile.Path);
                        continue;
                    }

                    Console.WriteLine("Copying " + sourceFile.Path);
                    await sourceFile.CopyAsync(destinationFolder, sourceFile.Name, NameCollisionOption.ReplaceExisting).AsTask();
                    Console.WriteLine("Copied " + sourceFile.Path);

                    phoneLibraryIndex.AddOrUpdateFile(new PhoneMusicFile { Path = path, Version = sourceVersion });
                    await phoneRepository.SavePhoneLibraryIndex(phoneLibraryIndex);
                }

                foreach (var fileToDelete in filesInDestinationFolder.Where(x => sourceFiles.All(sourceFile => sourceFile.Name != x.Name)))
                {
                    Console.WriteLine("Deleting file " + fileToDelete.Path);
                    await fileToDelete.DeleteAsync().AsTask();
                    Console.WriteLine("Deleted file " + fileToDelete.Path);
                }
            }

            foreach (var folderToDelete in destinationFolders.Where(x => sourceFolders.All(sourceFolder => sourceFolder.Name != x.Name)))
            {
                Console.WriteLine("Deleting folder " + folderToDelete.Path);
                await folderToDelete.DeleteAsync().AsTask();
                Console.WriteLine("Deleted folder " + folderToDelete.Path);
            }
        }

        private static string GetVersion(string sourceFilePath)
        {
            return new FileInfo(sourceFilePath).LastWriteTimeUtc.Ticks.ToString();
        }
    }
}
