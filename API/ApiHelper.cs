namespace API;

public static class ApiHelper
{
    public static string ExtensionToContentType(string extension)
    {
        return extension switch
        {
            ".html" or ".htm" => "text/html",
            ".css"            => "text/css",
            ".js"             => "application/javascript",
            ".json"           => "application/json",
            ".xml"            => "application/xml",
            ".txt"             => "text/plain",

            ".pdf"             => "application/pdf",

            ".png"             => "image/png",
            ".jpg" or ".jpeg"  => "image/jpeg",
            ".gif"             => "image/gif",
            ".svg"             => "image/svg+xml",
            ".webp"            => "image/webp",
            ".ico"             => "image/x-icon",

            ".mp3"             => "audio/mpeg",
            ".wav"             => "audio/wav",
            ".mp4"             => "video/mp4",
            ".webm"            => "video/webm",

            ".zip"             => "application/zip",
            ".csv"             => "text/csv",

            _                  => "application/octet-stream"
        };
    }
}