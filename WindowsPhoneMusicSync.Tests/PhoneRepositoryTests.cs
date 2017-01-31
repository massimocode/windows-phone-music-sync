using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowsPhoneMusicSync.Tests
{
    [TestClass]
    public class PhoneRepositoryTests
    {
        private const string PhoneName = "Windows phone";
        private PhoneRepository _phoneRepository;

        [TestInitialize]
        public void Setup()
        {
            _phoneRepository = new PhoneRepository(PhoneName);
        }

        [TestMethod]
        public async Task GetFolder_WhenFolderDoesNotExist_ShouldReturnNull()
        {
            // Act
            var folder = await _phoneRepository.GetFolder("Some_Folder_That_Doesnt_Exist");
            
            // Assert
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolder_ShouldGetMusicFolderFromRoot()
        {
            // Act
            var folder = await _phoneRepository.GetFolder("Music");

            // Assert
            Assert.IsNotNull(folder);
            Assert.AreEqual("Music", folder.Name);
        }

        [TestMethod]
        public async Task GetFolder_ShouldGetSubfolderFromMusicFolder()
        {
            // Act
            await _phoneRepository.CreateFolder(@"Music\Some_Test_Folder");

            // Act
            var folder = await _phoneRepository.GetFolder(@"Music\Some_Test_Folder");

            // Assert
            Assert.IsNotNull(folder);
            Assert.AreEqual("Some_Test_Folder", folder.Name);

            // Cleanup
            await _phoneRepository.DeleteFolder(@"Music\Some_Test_Folder");
        }

        [TestMethod]
        public async Task CreateFolder_ShouldCreateFolderInRoot_ShouldReturnCreatedFolder()
        {
            // Create Folder
            var folder = await _phoneRepository.CreateFolder("Some_Test_Folder");

            Assert.IsNotNull(folder);
            Assert.AreEqual("Some_Test_Folder", folder.Name);

            // Cleanup
            await _phoneRepository.DeleteFolder("Some_Test_Folder");
        }

        [TestMethod]
        public async Task CreateFolder_ShouldCreateFolderInSubfolder_ShouldReturnCreatedFolder()
        {
            // Act
            var folder = await _phoneRepository.CreateFolder(@"Music\From PC\Some_Test_Folder");

            // Assert
            Assert.IsNotNull(folder);
            Assert.AreEqual("Some_Test_Folder", folder.Name);

            // Cleanup
            await _phoneRepository.DeleteFolder(@"Music\From PC\Some_Test_Folder");
        }

        [TestMethod]
        public async Task CreateFolder_ShouldCreateFolderInProtectedFolder_ShouldReturnCreatedFolder()
        {
            // Act
            var folder = await _phoneRepository.CreateFolder(@"Music\Some_Test_Folder");

            // Assert
            Assert.IsNotNull(folder);
            Assert.AreEqual("Some_Test_Folder", folder.Name);

            // Cleanup
            await _phoneRepository.DeleteFolder(@"Music\Some_Test_Folder");
        }

        [TestMethod]
        public async Task CreateFolder_WhenFolderExists_ShouldReturnExistingFolder()
        {
            // Arrange
            var existingFolder = await _phoneRepository.CreateFolder("Some_Test_Folder");
            await Task.Delay(1000); // One second delay to ensure creation times are different as milliseconds aren't available!

            // Act
            var folder = await _phoneRepository.CreateFolder("Some_Test_Folder");

            // Assert
            Assert.IsNotNull(folder);
            Assert.AreEqual("Some_Test_Folder", folder.Name);
            Assert.AreEqual(existingFolder.DateCreated, folder.DateCreated);

            // Cleanup
            await _phoneRepository.DeleteFolder("Some_Test_Folder");
        }

        [TestMethod]
        public async Task DeleteFolder_ShouldDeleteFolderInRoot()
        {
            // Arrange
            await _phoneRepository.CreateFolder("Some_Test_Folder");

            // Act
            await _phoneRepository.DeleteFolder("Some_Test_Folder");

            // Assert
            var folder = await _phoneRepository.GetFolder("Some_Test_Folder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task DeleteFolder_ShouldDeleteFolderInSubDirectory()
        {
            // Arrange
            await _phoneRepository.CreateFolder(@"Music\From PC\Some_Test_Folder");

            // Act
            await _phoneRepository.DeleteFolder(@"Music\From PC\Some_Test_Folder");

            // Assert
            var folder = await _phoneRepository.GetFolder(@"Music\From PC\Some_Test_Folder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task DeleteFolder_ShouldDeleteFolderInProtectedDirectory()
        {
            // Arrange
            await _phoneRepository.CreateFolder(@"Music\Some_Test_Folder");

            // Act
            await _phoneRepository.DeleteFolder(@"Music\Some_Test_Folder");

            // Assert
            var folder = await _phoneRepository.GetFolder(@"Music\Some_Test_Folder");
            Assert.IsNull(folder);
        }
    }
}
