using RestSharp;
using System.Net;

internal class Program
{
    private static List<Thread> RedditStoryThreads = new List<Thread>();


    private static void Main(string[] args)
    {
        string subredditToSearch = args.Length == 1 ? args[0] : "r/stories";

        Console.WriteLine(FetchHtml($@"https://www.reddit.com/r/stories/"));

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
    }
    /// <summary>
    /// Fetch HTML from any website
    /// </summary>
    /// <param name="url">web url to search</param>
    /// <returns>This method returns the pure html of any page</returns>
    private static string FetchHtml(string url)
    {
        string result = string.Empty;
        try
        {
            var client = new RestClient("https://app.scrapingbee.com/api/v1/?api_key=CRLAGLQ7T7B0P9VM6F3LOHDCQH6FW2SFQ5CMZ1KF375S23R15STBGKOP0CJQD3B5126EIVC2DYAGVKGZ&url=https%3A%2F%2Fwww.reddit.com%2Fr%2Fstories%2F&block_ads=true");
            var request = new RestRequest();

            RestResponse response = client.Execute(request);

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+ "\\test.html", response.Content);
            Console.WriteLine(response.Content);

            //using (HttpClient client = new HttpClient())
            //{
            //    return client.GetStringAsync(url).Result;
            //}
            //WebRequest request = WebRequest.Create(HttpUtility.HtmlDecode(url));
            //using (WebResponse response = request.GetResponse())
            //{
            //    result = new StreamReader(response.GetResponseStream()).ReadToEnd();
            //}
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }

        return result;
    }
}

internal class RedditStory
{
    internal string Title { get; private set; }
    internal string Story { get; private set; }
    internal string Url { get; private set; }
    internal string Author { get; private set; }

    public RedditStory(string title, string story, string url, string author)
    {
        this.Title = title;
        this.Story = story;
        this.Url = url;
        this.Author = author;
    }
}