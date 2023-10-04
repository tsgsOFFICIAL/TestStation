using Github_Directory_Downloader;
using System.Diagnostics;

string _repositoryOwner = "tsgsOFFICIAL";
string _repositoryName = "CS2-AutoAccept.exe";
string _folderPath = "CS2-AutoAccept.exe/bin/Release/net6.0-windows/publish/win-x86";

string _repositoryUrl = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/contents";
string _targetDirectory = @"C:\Users\mmj\Desktop\C# test";

using (GitHubDirectoryDownloader downloader = new GitHubDirectoryDownloader(_repositoryOwner, _repositoryName, _folderPath))
{
    downloader.ProgressUpdated += UpdateProgress!;
    downloader.DownloadCompleted += DownloadCompleted!;

    await downloader.DownloadDirectoryAsync(_targetDirectory);
}

static void UpdateProgress(object sender, ProgressEventArgs e)
{
    Debug.WriteLine(e.Progress + "%");
}

static void DownloadCompleted(object sender, bool state)
{
    Debug.WriteLine(state ? "download finished" : "download failed");

    Console.WriteLine("Press any key to exit . . .");
    Console.ReadKey();
}