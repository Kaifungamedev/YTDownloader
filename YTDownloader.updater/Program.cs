using Octokit;
using System.Diagnostics;
using YTD.cli;
namespace YTD.Updater
{

    class Updater
    {
        public static async Task Main(string[] args)
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            var cli = new Cli();
            FileVersionInfo YTDinfo = FileVersionInfo.GetVersionInfo($"{exePath}/cli.dll");
            string[] YTDversion = YTDinfo.ProductVersion.Split('.');
            var repo = new GitHubClient(new ProductHeaderValue("YTDownloader"));
            var releases = await repo.Repository.Release.GetLatest("Kaifungamedev", "YTDownloader");
            string[] releselatest = releases.TagName.Split(".");
            string releseName = releases.Name;
            int index = 0;
            foreach (var _ in YTDversion)
            {
                if (int.Parse(releselatest[index]) > int.Parse(YTDversion[index]))
                {
                    string Updatepath = $"https://github.com/kaifungamedev/YTDownloader/releases/download/{releases.TagName}/{getostype()}_cli.dll";
                    Console.WriteLine($"new version avalable updating {Updatepath}", releases.TagName, getostype());
                    var http = new HttpClient();
                    var updateFile = await http.GetByteArrayAsync(Updatepath);
                    await File.WriteAllBytesAsync($"{exePath}/cli.dll", updateFile);
                    string[] tmpargs = { "-h" };

                    await cli.cli(tmpargs);
                    Environment.Exit(0);
                }
            }
            await cli.cli(args);

        }
        static string getostype()
        {
            if (OperatingSystem.IsLinux())
            {
                return "Linux";
            }
            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }
            return "unknown";
        }
    }
}