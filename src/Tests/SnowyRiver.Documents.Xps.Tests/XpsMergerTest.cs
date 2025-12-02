using System.IO;

namespace SnowyRiver.Documents.Xps.Tests;

public class XpsMergerTest
{
    [WpfFact]
    public async Task TestMergeXpsBytes()
    {
        const string basePath = "Assets";
        var bytes1 = await File.ReadAllBytesAsync(Path.Combine(basePath, "test1.xps"));
        var bytes2 = await File.ReadAllBytesAsync(Path.Combine(basePath, "test2.xps"));
        var mergedBytes = XpsMerger.Merge([bytes1, bytes2]);
        await WriteAllBytesToFile("merged.xps", mergedBytes);
    }

    private async Task WriteAllBytesToFile(string fileName, byte[] bytes, CancellationToken cancellationToken = default)
    {
        const string outputPath = "Output";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        var filePath = Path.Combine(outputPath, fileName);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }
}
