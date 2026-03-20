namespace SnowyRiver.MediaTypes;

public class MediaType(string name, string mediaType, string extension)
{
    public string Name => name;
    public string Type => mediaType;
    public string Extension => extension;

    // Image
    public static MediaType Bitmap { get; } = new("Bitmap", "image/bmp", ".bmp");
    public static MediaType Gif { get; } = new("GIF", "image/gif", ".gif");
    public static MediaType Jpeg { get; } = new("JPEG", "image/jpeg", ".jpg");
    public static MediaType Png { get; } = new("PNG", "image/png", ".png");
    public static MediaType Webp { get; } = new("WebP", "image/webp", ".webp");
    public static MediaType Svg { get; } = new("SVG", "image/svg+xml", ".svg");
    public static MediaType Icon { get; } = new("Icon", "image/x-icon", ".ico");
    public static MediaType Tiff { get; } = new("TIFF", "image/tiff", ".tif");

    // Text / Data
    public static MediaType PlainText { get; } = new("PlainText", "text/plain", ".txt");
    public static MediaType Html { get; } = new("HTML", "text/html", ".html");
    public static MediaType Css { get; } = new("CSS", "text/css", ".css");
    public static MediaType JavaScript { get; } = new("JavaScript", "text/javascript", ".js");
    public static MediaType Json { get; } = new("JSON", "application/json", ".json");
    public static MediaType Xml { get; } = new("XML", "application/xml", ".xml");

    // Application
    public static MediaType Pdf { get; } = new("PDF", "application/pdf", ".pdf");
    public static MediaType ExeWindows { get; } = new("Exe (Windows)", "application/octet-stream", ".exe");
    public static MediaType RichTextFormat { get; } = new("Rich Text Format", "application/rtf", ".rtf");
    public static MediaType OutlookMessage { get; } = new("Outlook Message", "application/vnd.ms-outlook", ".msg");
    public static MediaType Xps { get; } = new("Xps", "application/vnd.ms-xpsdocument", ".xps");
    public static MediaType OpenDocumentPresentation { get; } = new("Open Document Presentation", "application/vnd.oasis.opendocument.presentation", ".odp");
    public static MediaType OpenDocumentSpreadsheet { get; } = new("Open Document Spreadsheet", "application/vnd.oasis.opendocument.spreadsheet", ".ods");
    public static MediaType OpenDocumentText { get; } = new("Open Document Text", "application/vnd.oasis.opendocument.text", ".odt");

    public static MediaType Word { get; } = new("Word", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");
    public static MediaType WordTemplate { get; } = new("Word template", "application/vnd.openxmlformats-officedocument.wordprocessingml.template", ".dotx");
    public static MediaType Word97To2003 { get; } = new("Word 97-2003", "application/msword", ".doc");
    public static MediaType WordOpenXml { get; } = new("WordOpenXml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");

    public static MediaType Excel { get; } = new("Excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
    public static MediaType Excel97To2003 { get; } = new("Excel 97-2003", "application/vnd.ms-excel", ".xls");
    public static MediaType ExcelOpenXml { get; } = new("ExcelOpenXml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");

    public static MediaType PowerPoint { get; } = new("PowerPoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx");
    public static MediaType PowerPoint97To2003 { get; } = new("Powerpoint 97-2003", "application/vnd.ms-powerpoint", ".ppt");

    public static MediaType Visio { get; } = new("Visio", "application/vnd.visio", ".vsdx");
    public static MediaType Visio97To2003 { get; } = new("Visio 97-2003", "application/vnd.visio", ".vsd");

    public static MediaType Zip { get; } = new("ZIP", "application/zip", ".zip");
    public static MediaType GZip { get; } = new("GZip", "application/gzip", ".gz");
    public static MediaType SevenZip { get; } = new("7z", "application/x-7z-compressed", ".7z");
    public static MediaType Binary { get; } = new("Binary", "application/octet-stream", ".bin");

    // Audio
    public static MediaType Mp3 { get; } = new("MP3", "audio/mpeg", ".mp3");
    public static MediaType Wav { get; } = new("WAV", "audio/wav", ".wav");
    public static MediaType OggAudio { get; } = new("OggAudio", "audio/ogg", ".ogg");
    public static MediaType Flac { get; } = new("FLAC", "audio/flac", ".flac");

    // Video
    public static MediaType Mp4 { get; } = new("MP4", "video/mp4", ".mp4");
    public static MediaType ThreeGpp { get; } = new("3GPP", "video/3gpp", ".3gp");
    public static MediaType QuickTime { get; } = new("QuickTime", "video/quicktime", ".mov");
    public static MediaType Webm { get; } = new("WebM", "video/webm", ".webm");
    public static MediaType Avi { get; } = new("AVI", "video/x-msvideo", ".avi");
    public static MediaType Mov { get; } = new("MOV", "video/quicktime", ".mov");

}
