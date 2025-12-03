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

        var targetPackageUriPath = $"memory://target_{Guid.NewGuid()}.xps";
        var targetPackageUri = new Uri(targetPackageUriPath);
        using var targetPackage = Package.Open(mergedStream, FileMode.Create, FileAccess.ReadWrite);
        using var mergedXpsDoc =
            new XpsDocument(targetPackage, CompressionOption.Maximum, targetPackageUriPath);
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
            using var sourcePackage = Package.Open(sourceStream, FileMode.Open, FileAccess.Read);
            using var sourceXpsDoc = new XpsDocument(sourcePackage, CompressionOption.Normal, sourcePackageUriPath);
            PackageStore.AddPackage(sourcePackageUri, sourcePackage);
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
                    ((IUriContext)targetDocumentReference).BaseUri = ((IUriContext)sourceDocumentReference).BaseUri;
                    var fixedDocument = targetDocumentReference.GetDocument(true);
                    if (fixedDocument != null && fixedDocument.Pages.Any())
                    {
                        targetDocumentReference.SetDocument(fixedDocument);
                        documentSequence.References.Add(targetDocumentReference);
                    }
                }
            }
            PackageStore.RemovePackage(sourcePackageUri);
        }

        // 写入合并后的文档序列
        if (documentSequence.References.Count > 0)
        {
            xpsWriter.Write(documentSequence);
        }
        PackageStore.RemovePackage(targetPackageUri);

        mergedStream.Position = 0;
        return mergedStream;
    }
}
