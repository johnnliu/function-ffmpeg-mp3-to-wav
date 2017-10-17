#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Blob;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;

public static HttpResponseMessage Run(Stream req, TraceWriter log)
{

    var temp = Path.GetTempFileName() + ".mp3";
    var tempOut = Path.GetTempFileName() + ".wav";
    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    Directory.CreateDirectory(tempPath);

    using (var ms = new MemoryStream())
    {
        req.CopyTo(ms);
        File.WriteAllBytes(temp, ms.ToArray());
    }

    var bs = File.ReadAllBytes(temp);
    log.Info($"Renc Length: {bs.Length}");


    var psi = new ProcessStartInfo();
    psi.FileName = @"D:\home\site\wwwroot\mp3-to-wave\ffmpeg.exe";
    psi.Arguments = $"-i \"{temp}\" \"{tempOut}\"";
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;
    psi.UseShellExecute = false;
    
    log.Info($"Args: {psi.Arguments}");
    var process = Process.Start(psi);
    process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);


    var bytes = File.ReadAllBytes(tempOut);
    log.Info($"Renc Length: {bytes.Length}");


    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content = new StreamContent(new MemoryStream(bytes));
    response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

    File.Delete(tempOut);
    File.Delete(temp);
    Directory.Delete(tempPath, true);    

    return response;
}
