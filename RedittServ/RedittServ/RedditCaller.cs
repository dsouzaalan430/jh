using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace RedittServ
{
    class RedditCaller
    {
        private static HttpClient client = new HttpClient();
        private static string subReditt = "olympics"; 
        private static Dictionary<string, int> userPostNumber = new Dictionary<string, int>();
        private static List<(string postId, int upvotes)> postVotes = new List<(string, int)>();
        private static System.Timers.Timer timer;

        static async Task Main(string[] args)
        {

            timer = new System.Timers.Timer(60000); // 60 seconds
            timer.Elapsed += ShowStatistics;
            timer.Start();

            while (true)
            {
                await GetPosts();
                await Task.Delay(3000); // Wait for 3 seconds for rate limits
            }
        }

        private static async Task GetPosts()
        {
            var url = $"https://reddit.com/r/{subReditt}/new.json?limit=100";
            var response = await client.GetStringAsync(url);
            var jsonResponse = JsonDocument.Parse(response).RootElement;
            var posts = jsonResponse.GetProperty("data").GetProperty("children").EnumerateArray();

            foreach (var post in posts)
            {
                var postData = post.GetProperty("data");
                string userId = postData.GetProperty("author").GetString();
                string postId = postData.GetProperty("id").GetString();
                int upvotes = postData.GetProperty("ups").GetInt32();

                // Track user post counts
                if (userPostNumber.ContainsKey(userId))
                    userPostNumber[userId]++;
                else
                    userPostNumber[userId] = 1;

                // Track post upvotes
                postVotes.Add((postId, upvotes));
            }
        }

        private static void ShowStatistics(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Statistics:");

            // Top 5 posts by upvotes
            var topPosts = postVotes.OrderByDescending(x => x.upvotes).Take(5);
            Console.WriteLine("Top 5 Posts by Upvotes:");
            foreach (var post in topPosts)
                Console.WriteLine($"Post ID: {post.postId}, Upvotes: {post.upvotes}");

            // Top 5 users by number of posts
            var topUsers = userPostNumber.OrderByDescending(x => x.Value).Take(5);
            Console.WriteLine("Top 5 Users by Post Count:");
            foreach (var user in topUsers)
                Console.WriteLine($"User: {user.Key}, Posts: {user.Value}");
        }
    }
}