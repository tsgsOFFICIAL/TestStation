using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

string repositoryOwner = "tsgsOFFICIAL";
string repositoryName = "CS2-AutoAccept.exe";
string folderPath = "CS2-AutoAccept.exe/bin/Release/net6.0-windows/publish/win-x86";
string downloadDirectory = "C:\\Users\\mmj\\Desktop\\C# Test";

string apiUrl = $"https://api.github.com/repos/{repositoryOwner}/{repositoryName}/contents/{folderPath}";

long totalSize = 0;
long downloadedSize = 0;

using (HttpClient client = new HttpClient())
{
    IProgress<int> progress = new Progress<int>(percentComplete =>
    {
        Debug.WriteLine($"Progress: {percentComplete}%");
    });

    client.DefaultRequestHeaders.Add("User-Agent", "request");
    await CalculateFolderSize(client, apiUrl, downloadDirectory);
    await DownloadFolderContents(client, apiUrl, downloadDirectory, progress);
}

// Download completed
Console.WriteLine("Download completed!");

async Task CalculateFolderSize(HttpClient client, string apiUrl, string downloadDirectory)
{
    HttpResponseMessage response = await client.GetAsync(apiUrl);

    if (response.IsSuccessStatusCode)
    {
        string json = await response.Content.ReadAsStringAsync();
        GitHubContent[] contents = JsonSerializer.Deserialize<GitHubContent[]>(json)!;
        totalSize += (long)contents.Sum(content => content.size)!;

        foreach (GitHubContent content in contents)
        {
            if (content.type == "dir")
            {
                string subfolderPath = content.path!;
                await CalculateFolderSize(client, apiUrl.Replace(folderPath, subfolderPath), downloadDirectory);
            }
        }
    }
    else
    {
        Console.WriteLine($"Failed to fetch folder contents. Status code: {response.StatusCode}");
    }
}
async Task DownloadFolderContents(HttpClient client, string apiUrl, string downloadDirectory, IProgress<int> progress)
{
    HttpResponseMessage response = await client.GetAsync(apiUrl);

    if (response.IsSuccessStatusCode)
    {
        string json = await response.Content.ReadAsStringAsync();
        GitHubContent[] contents = JsonSerializer.Deserialize<GitHubContent[]>(json)!;

        if (!Directory.Exists(downloadDirectory))
        {
            Directory.CreateDirectory(downloadDirectory);
        }

        foreach (GitHubContent content in contents)
        {
            if (content.type == "file")
            {

                string fileUrl = content.download_url!;
                string filePath = Path.Combine(downloadDirectory, content.name!);

                using (HttpResponseMessage fileResponse = await client.GetAsync(fileUrl))
                {
                    if (fileResponse.IsSuccessStatusCode)
                    {
                        byte[] bytes = await fileResponse.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(filePath, bytes);
                        Console.WriteLine($"Downloaded {content.name}");

                        // Increment downloadedSize by the size of the downloaded file
                        downloadedSize += bytes.Length;

                        // Calculate progress as a percentage of downloadedSize relative to totalSize
                        int percentComplete = (int)(((double)downloadedSize / totalSize) * 100);
                        progress.Report(percentComplete);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to download {content.name}");
                    }
                }
            }
            else if (content.type == "dir")
            {

                string subfolderPath = content.path!;
                string subfolderDownloadDirectory = Path.Combine(downloadDirectory, content.name!);
                await DownloadFolderContents(client, apiUrl.Replace(folderPath, subfolderPath), subfolderDownloadDirectory, progress);
            }
        }
    }
    else
    {
        Console.WriteLine($"Failed to fetch folder contents. Status code: {response.StatusCode}");
    }
}

public class GitHubContent
{
    public string? name { get; set; }
    public string? path { get; set; }
    public string? type { get; set; }
    public string? download_url { get; set; }
    public long? size { get; set; }
}
