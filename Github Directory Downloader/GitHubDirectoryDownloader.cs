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
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;
        private readonly string _folderPath;
        private long _totalFileSize = 0;
        private long _downloadedFileSize = 0;
        private object _lockTotalSize = new object();
        private object _lockDownloadedSize = new object();
        private object _lockDownloadTasks = new object();
        private object _lockSubfolderTasks = new object();
        private List<Task> _downloadTasks;
        private List<Task> _subfolderTasks;
        public event EventHandler<ProgressEventArgs>? ProgressUpdated;
        public event EventHandler<bool>? DownloadCompleted;

        /// <summary>
        /// Initializes a new instance of the GitHubDirectoryDownloader class.
        /// </summary>
        /// <param name="repositoryOwner">Repository Owner</param>
        /// <param name="repositoryName">Repository Name</param>
        /// <param name="folderPath">Folder path in repository</param>
        public GitHubDirectoryDownloader(string repositoryOwner, string repositoryName, string folderPath)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHubDirectoryDownloader");
            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;
            _folderPath = folderPath;
            _downloadTasks = new List<Task>();
            _subfolderTasks = new List<Task>();
        }

        /// <summary>
        /// Downloads a GitHub directory asynchronously
        /// </summary>
        /// <param name="downloadPath">Where to download the directory to</param>
        /// <returns>This method returns a Task, meaning it's awaitable</returns>
        public async Task DownloadDirectoryAsync(string downloadPath, string apiUrl = null!)
        {
            // Construct the API url
            apiUrl = apiUrl ?? $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/contents/{_folderPath}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                GitHubContent[] contents = JsonSerializer.Deserialize<GitHubContent[]>(json) ?? Array.Empty<GitHubContent>();

                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                }

                lock (_lockTotalSize)
                {
                    _totalFileSize += contents.Sum(content => content.Size ?? 0);
                }

                foreach (GitHubContent item in contents)
                {
                    switch (item.Type)
                    {
                        case "file":
                            string downloadUrl = item.DownloadUrl!;
                            string localFilePath = Path.Combine(downloadPath, item.Name!);

                            lock (_lockDownloadTasks)
                            {
                                _downloadTasks.Add(DownloadFileAsync(downloadUrl!, localFilePath));
                            }
                            break;
                        case "dir":
                            string subFolderPath = item.Path!;
                            string subfolderDownloadDirectory = Path.Combine(downloadPath, item.Name!);

                            lock (_subfolderTasks)
                            {
                                _subfolderTasks.Add(DownloadDirectoryAsync(subfolderDownloadDirectory, apiUrl.Replace(_folderPath, subFolderPath)));
                            }
                            break;
                    }
                }

                Debug.WriteLine("waiting for tasks to complete");
                await Task.WhenAll(_downloadTasks);
                await Task.WhenAll(_subfolderTasks);
            }
            else
            {
                OnDownloadChanged(false);
                Dispose();
            }
        }

        /// <summary>
        /// Downloads a file from a URL and saves it to a local file path.
        /// </summary>
        /// <param name="downloadUrl">The URL of the file to download.</param>
        /// <param name="filePath">The local file path where the downloaded file will be saved.</param>
        private async Task DownloadFileAsync(string downloadUrl, string filePath)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

                if (response.IsSuccessStatusCode)
                {
                    using FileStream fileStream = File.Create(filePath);
                    await response.Content.CopyToAsync(fileStream);

                    lock (_lockDownloadedSize)
                    {
                        _downloadedFileSize += fileStream.Length;
                        OnProgressChanged(new ProgressEventArgs((int)(((double)_downloadedFileSize / _totalFileSize) * 100)));
                    }
                }
                else
                {
                    Debug.WriteLine($"Failed to download a file: {response.Content}");
                }
            }
            catch (Exception)
            {
                OnDownloadChanged(false);
                Dispose();
            }
        }

        /// <summary>
        /// Raises the ProgressChanged event.
        /// </summary>
        /// <param name="e">An instance of ProgressEventArgs, can hold Progress as an int (0-100).</param>
        protected virtual void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressUpdated?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the DownloadCompleted event.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnDownloadChanged(bool state)
        {
            DownloadCompleted?.Invoke(this, state);
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
    /// <summary>
    /// EventArgs for progress update
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Progress 0-100%
        /// </summary>
        public int Progress { get; set; }
        /// <summary>
        /// Status, either good or bad
        /// </summary>
        public string Status { get; set; }
        public ProgressEventArgs(int progress, string status = "default")
        {
            Progress = progress;
            Status = status;
        }
    }
}