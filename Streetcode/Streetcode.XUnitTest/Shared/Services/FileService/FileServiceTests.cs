namespace Streetcode.XUnitTest.Shared.Services.FileService;

using System.Text;
using Xunit;
using FileService = global::Streetcode.Shared.Services.FileService;

public class FileServiceTests
{
    private const string TestKey = "12345678901234561234567890123456"; // 32 bytes

    [Fact]
    public void EncryptAndDecrypt_ShouldReturnOriginalBytes()
    {
        // Arrange
        byte[] original = Encoding.UTF8.GetBytes("Hello World");

        // Act
        byte[] encrypted = FileService.EncryptBytes(original, TestKey);
        byte[] decrypted = FileService.DecryptBytes(encrypted, TestKey);

        // Assert
        Assert.NotEqual(original, encrypted);
        Assert.Equal(original, decrypted);
        Assert.Equal(16 + 16, encrypted.Length); // IV (16) + Padded AES block (16)
    }

    [Fact]
    public void HashFunction_ShouldReplaceSlashes()
    {
        // Arrange
        string input = "some-input";

        // Act
        string hash = FileService.HashFunction(input);

        // Assert
        Assert.DoesNotContain("/", hash);
        Assert.Contains("_", hash);
    }

    [Fact]
    public void PrepareFileStorageName_ShouldRemoveInvalidCharacters()
    {
        // Arrange
        string input = "my.photo:test.png";

        // Act
        string result = FileService.PrepareFileStorageName(input);

        // Assert
        Assert.DoesNotContain(".", result);
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain(" ", result);
    }
}