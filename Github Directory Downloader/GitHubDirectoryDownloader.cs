using System.Diagnostics;
using System.Text.Json;

namespace Github_Directory_Downloader
{
    /// <summary>
    /// Downloads a directory from a GitHub repository with support for async operations, progress reporting, and proper disposal.
    /// </summary>
    public class GitHubDirectoryDownloader : IDisposable
    {
        private readonly HttpClient _httpClient;
        private string _apiUrl;
        private string _directoryPath;
        private string _downloadDirectory;
        private List<Task> _downloadTasks;
        private List<Task> _subfolderTasks;

        /// <summary>
        /// Occurs when the download progress changes.
        /// </summary>
        public event EventHandler<int>? ProgressChanged;

        /// <summary>
        /// Initializes a new instance of the GitHubDirectoryDownloader class.
        /// </summary>
        public GitHubDirectoryDownloader(string apiUrl, string directoryPath, string downloadDirectory)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHubDirectoryDownloader");
            _apiUrl = apiUrl;
            _directoryPath = directoryPath;
            _downloadDirectory = downloadDirectory;
            _downloadTasks = new();
            _subfolderTasks = new();
        }

        /// <summary>
        /// Downloads the directory asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task DownloadDirectoryAsync()
        {
            await DownloadDirectoryContentAsync($"{_apiUrl}/{_directoryPath}", _downloadDirectory);
            await Task.WhenAll(_downloadTasks);
            await Task.WhenAll(_subfolderTasks);
        }

        /// <summary>
        /// Downloads the directory content asynchronously.
        /// </summary>
        /// <param name="apiUrl">GitHub API apiUrl</param>
        /// <param name="directoryPath"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        public async Task DownloadDirectoryContentAsync(string apiUrl, string downloadDirectory)
        {
            try
            {
                string repoContentsUrl = apiUrl.TrimEnd('/');

                GitHubContent[] directoryContents = await GetDirectoryContentsAsync(repoContentsUrl) ?? Array.Empty<GitHubContent>();

                if (!Directory.Exists(downloadDirectory))
                {
                    Directory.CreateDirectory(downloadDirectory);
                }

                foreach (GitHubContent item in directoryContents)
                {
                    switch (item.Type)
                    {
                        case "file":
                            string downloadUrl = item.DownloadUrl!;
                            string localFilePath = Path.Combine(downloadDirectory, item.Name!);

                            _downloadTasks.Add(DownloadFileAsync(downloadUrl!, localFilePath));
                            await Task.Delay(500);
                            break;
                        case "dir":
                            string subFolderPath = item.Path!;
                            string subfolderDownloadDirectory = Path.Combine(downloadDirectory, item.Name!);

                            _subfolderTasks.Add(DownloadDirectoryContentAsync(apiUrl.Replace(_directoryPath, subFolderPath), subfolderDownloadDirectory));
                            await Task.Delay(500);
                            break;
                    }

                    OnProgressChanged((int)((double)Array.IndexOf(directoryContents, item) / directoryContents.Length * 100));
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Gets the contents of a directory from the GitHub repository.
        /// </summary>
        /// <param name="apiUrl">The URL of the directory contents in the GitHub repository.</param>
        /// <returns>An array of GitHubContent objects representing directory items.</returns>
        private async Task<GitHubContent[]> GetDirectoryContentsAsync(string apiUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            GitHubContent[] directoryContents = JsonSerializer.Deserialize<GitHubContent[]>(content) ?? Array.Empty<GitHubContent>();

            return directoryContents;
        }

        /// <summary>
        /// Downloads a file from a URL and saves it to a local file path.
        /// </summary>
        /// <param name="downloadUrl">The URL of the file to download.</param>
        /// <param name="filePath">The local file path where the downloaded file will be saved.</param>
        private async Task DownloadFileAsync(string downloadUrl, string filePath)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                using FileStream fileStream = File.Create(filePath);
                await response.Content.CopyToAsync(fileStream);
            }
            else
            {
                Debug.WriteLine($"Failed to download a file: {response.Content}");
            }
        }

        /// <summary>
        /// Raises the ProgressChanged event.
        /// </summary>
        /// <param name="progress">The current progress value (0-100).</param>
        protected virtual void OnProgressChanged(int progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        /// <summary>
        /// Disposes of the GitHubDirectoryDownloader instance.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}