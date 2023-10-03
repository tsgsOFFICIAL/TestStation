using Github_Directory_Downloader;

string _repositoryOwner = "tsgsOFFICIAL";
string _repositoryName = "CS2-AutoAccept.exe";
string _directoryPath = "CS2-AutoAccept.exe/bin/Release/net6.0-windows/publish/win-x86";

string _repositoryUrl = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/contents";
string _targetDirectory = @"C:\Users\Marcus\AppData\Roaming\CS2 AutoAccept";

using (GitHubDirectoryDownloader downloader = new(_repositoryUrl, _directoryPath, _targetDirectory))
{
    downloader.ProgressChanged += (sender, progress) =>
    {
        Console.WriteLine($"Download Progress: {progress}%");
    };

    await downloader.DownloadDirectoryAsync();
}