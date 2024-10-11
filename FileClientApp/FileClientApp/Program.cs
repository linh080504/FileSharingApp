using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string serverUrl = "https://localhost:7132/api/file"; // Địa chỉ của server

        // Gửi file lên server
        var filePath = "C:\\Users\\vuluy\\Downloads\\HTPT.docx"; // Đường dẫn tới file bạn muốn gửi
        await UploadFileAsync(serverUrl, filePath);

        // Nhận file từ server
        var fileName = "HTPT.docx"; // Tên file bạn muốn tải về
        await DownloadFileAsync(serverUrl, fileName);
    }

    // Gửi file tới server
    static async Task UploadFileAsync(string serverUrl, string filePath)
    {
        using (var client = new HttpClient())
        {
            using (var form = new MultipartFormDataContent())
            {
                var fileStream = new FileStream(filePath, FileMode.Open);
                var fileContent = new StreamContent(fileStream);
                form.Add(fileContent, "file", Path.GetFileName(filePath));

                var response = await client.PostAsync($"{serverUrl}/upload", form);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("File uploaded successfully.");
                }
                else
                {
                    Console.WriteLine("Error uploading file.");
                }
            }
        }
    }

    // Tải file từ server
    static async Task DownloadFileAsync(string serverUrl, string fileName)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"{serverUrl}/download/{fileName}");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var downloadPath = Path.Combine("downloads", fileName); // Tạo thư mục 'downloads' để lưu file
                Directory.CreateDirectory("downloads");

                await File.WriteAllBytesAsync(downloadPath, fileBytes);
                Console.WriteLine($"File downloaded to {downloadPath}");
            }
            else
            {
                Console.WriteLine("Error downloading file.");
            }
        }
    }
}
