using System.IO;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;

namespace SnowyRiver.Documents.Xps;

public static class XpsMerger
{
    public static byte[]? Merge(IEnumerable<byte[]?>? sourceXpsBytes)
    {
        if (sourceXpsBytes == null)
        {
            return null;
        }

        var memoryStreams = sourceXpsBytes?
            .Where(x => x is { Length: > 0 })
            .Select(bytes =>
            {
                var ms = new MemoryStream();
                ms.Write(bytes!, 0, bytes!.Length);
                ms.Position = 0;
                return ms;
            })
            .ToArray();
        if (memoryStreams == null || memoryStreams.Length == 0)
        {
            throw new ArgumentException("源文档字节数组列表不能为空。");
        }
        using var mergedStream = Merge(memoryStreams);
        foreach (var stream in memoryStreams)
        {
            stream.Dispose();
        }
        return mergedStream.ToArray();
    }

    public static MemoryStream Merge(IEnumerable<MemoryStream> sourceXpsStreams)
    {
        var mergedStream = new MemoryStream();

        var sourceUriAddedToStore = new List<Uri>();
        var sourcePackages = new List<Package>();
        var sourceDocument = new List<XpsDocument>();
        var targetPackageUriPath = $"memory://target_{Guid.NewGuid()}.xps";
        var targetPackageUri = new Uri(targetPackageUriPath);
        using var targetPackage = Package.Open(mergedStream, FileMode.Create, FileAccess.ReadWrite);
        using var mergedXpsDoc = new XpsDocument(targetPackage, CompressionOption.Maximum, targetPackageUriPath);
        try
        {

            PackageStore.AddPackage(targetPackageUri, targetPackage);

            var xpsWriter = XpsDocument.CreateXpsDocumentWriter(mergedXpsDoc);
            var documentSequence = new FixedDocumentSequence();

            foreach (var sourceStream in sourceXpsStreams)
            {
                if (sourceStream.Length == 0)
                    continue;

                sourceStream.Position = 0;

                // 为每个源文档创建唯一的URI
                var sourcePackageUriPath = $"memory://source_{Guid.NewGuid()}.xps";
                var sourcePackageUri = new Uri(sourcePackageUriPath);
                var sourcePackage = Package.Open(sourceStream, FileMode.Open, FileAccess.Read);
                sourcePackages.Add(sourcePackage);
                var sourceXpsDoc = new XpsDocument(sourcePackage, CompressionOption.Normal, sourcePackageUriPath);
                sourceDocument.Add(sourceXpsDoc);
                PackageStore.AddPackage(sourcePackageUri, sourcePackage);
                sourceUriAddedToStore.Add(sourcePackageUri);
                var sourceSequence = sourceXpsDoc.GetFixedDocumentSequence();
                if (sourceSequence != null)
                {
                    // 复制文档引用
                    foreach (var sourceDocumentReference in sourceSequence.References)
                    {
                        var targetDocumentReference = new DocumentReference
                        {
                            Source = sourceDocumentReference.Source
                        };
                        // 设置BaseUri以确保资源解析正确
                        ((IUriContext)targetDocumentReference).BaseUri = ((IUriContext)sourceDocumentReference).BaseUri;
                        documentSequence.References.Add(targetDocumentReference);
                    }
                }
            }

            // 写入合并后的文档序列
            if (documentSequence.References.Count > 0)
            {
                xpsWriter.Write(documentSequence);
            }

            mergedStream.Position = 0;
            return mergedStream;
        }
        finally
        {
            PackageStore.RemovePackage(targetPackageUri);
            foreach (var sourceUri in sourceUriAddedToStore)
            {
                PackageStore.RemovePackage(sourceUri);
            }

            foreach (var document in sourceDocument)
            {
                document.Close();
            }

            foreach (var package in sourcePackages)
            {
                package.Close();
            }
        }
    }
}
