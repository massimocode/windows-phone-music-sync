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

            var localMusicFolder = KnownFolders.MusicLibrary;

            var phoneMusicFolder = await PhoneRepository.GetPhoneMusicFolder(phoneName);
            var phoneLibraryIndex = await PhoneRepository.GetPhoneLibraryIndex(phoneName);

            var sourceFolders = await localMusicFolder.GetFoldersAsync().AsTask();
            foreach (var sourceFolder in sourceFolders)
            {
                var destinationFolder = await phoneMusicFolder.CreateFolderAsync(sourceFolder.Name, CreationCollisionOption.OpenIfExists).AsTask();
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
                    await PhoneRepository.SavePhoneLibraryIndex(phoneMusicFolder, phoneLibraryIndex);
                }
            }
        }

        private static string GetVersion(string sourceFilePath)
        {
            return new FileInfo(sourceFilePath).LastWriteTimeUtc.Ticks.ToString();
        }
    }
}
