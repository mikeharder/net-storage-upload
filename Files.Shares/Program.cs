using Azure;
using Azure.Storage.Files.Shares;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Files.Shares
{
    class Program
    {
        private const int _size = 1024 * 1024 * 1024;
        private const int BYTES_PER_MEGABYTE = 1024 * 1024;

        private static int _uploadsCompleted;

        static async Task Main(string[] args)
        {
            var statusThread = new Thread(() =>
            {
                while (true)
                {
                    var process = Process.GetCurrentProcess();
                    var workingSetMB = ((double)process.WorkingSet64) / (BYTES_PER_MEGABYTE);
                    var privateMemoryMB = ((double)process.PrivateMemorySize64) / (BYTES_PER_MEGABYTE);
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] Uploads: {_uploadsCompleted}, " +
                        $"Working Set: {workingSetMB:N2}M, Private Memory: {privateMemoryMB:N2}M");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });
            statusThread.Start();

            var connectionString = Environment.GetEnvironmentVariable("FILES_CONNECTION_STRING");

            var shareClient = new ShareClient(connectionString, "test");
            var directoryClient = shareClient.GetDirectoryClient("test");
            var fileClient = directoryClient.GetFileClient("test");

            var stream = new MemoryStream(new byte[_size]);
            await fileClient.CreateAsync(stream.Length);

            while (true)
            {
                stream.Seek(0, SeekOrigin.Begin);
                await fileClient.UploadAsync(stream);
                _uploadsCompleted++;
            }
        }
    }
}
