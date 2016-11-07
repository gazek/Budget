using Budget.API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Budget.Tests.Services
{
    [TestClass]
    public class AesServiceTest
    {

        [TestMethod]
        public void AesEncrytDecrypt()
        {
            // Arrange
            byte[] key = new byte[32];
            byte[] IV = new byte[16];
            string plainText = "encryption test string";
            byte[] encryptedValue;
            string decrypyedValue;

            // Act
            encryptedValue = AesService.EncryptStringToBytes(plainText, key, IV);
            decrypyedValue = AesService.DecryptStringFromBytes(encryptedValue, key, IV);

            // Assert
            Assert.AreEqual(plainText, decrypyedValue);
            Assert.IsInstanceOfType(encryptedValue, typeof(byte[]));
            Assert.IsTrue(encryptedValue.Length > 0);

        }

    }
}
