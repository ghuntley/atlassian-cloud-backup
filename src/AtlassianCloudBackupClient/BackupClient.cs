using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Conditions;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace AtlassianCloudBackupClient
{
    public interface IBackupClient
    {
        Task DownloadConfluenceBackupAsync(Uri destinationDirectory, DateTime backupDate);

        Task DownloadJiraBackupAsync(Uri destinationDirectory, DateTime backupDate);

        Task RequestConfluenceBackupAsync();

        Task RequestJiraBackupAsync();
    }

    public class BackupClient : IBackupClient
    {
        private static ILogger logger = Log.ForContext<BackupClient>();

        public BackupClient(Uri instanceAddress, NetworkCredential credentials)
        {
            InstanceAddress = instanceAddress;
            Credentials = credentials;

            Condition.Requires(InstanceAddress, "InstanceAddress")
                .IsNotNull()

                // Refactor this safety check when https://jira.atlassian.com/browse/CLOUD-6999 is resolved.
                .Evaluate(x => x.Host.EndsWith("atlassian.net") || x.Host.EndsWith("jira.com"));

            Condition.Requires(Credentials, "Credentials").IsNotNull();
            Condition.Requires(Credentials.UserName, "Credentials Username").IsNotNullOrWhiteSpace();
            Condition.Requires(Credentials.Password, "Credentials Password").IsNotNullOrWhiteSpace();
        }

        public CookieContainer Cookies { get; private set; }

        public NetworkCredential Credentials { get; private set; }

        public Uri InstanceAddress { get; private set; }

        public async Task<CookieContainer> AuthenticateAsync()
        {
            logger.Information("Signing in as {username} at {url}", Credentials.UserName, InstanceAddress.Host);

            using (var client = BuildHttpClient())
            {
                var byteArray =
                    Encoding.ASCII.GetBytes(String.Format("{0}:{1}", Credentials.UserName, Credentials.Password));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(byteArray));

                var url = String.Format("https://{0}/Dashboard.jspa", InstanceAddress.Host);

                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                Cookies = response.ReadCookies();
            }

            Condition.Ensures(Cookies.Count > 1);

            logger.Information("{username} has successfully authenticated.", Credentials.UserName);

            return Cookies;
        }

        public async Task DownloadConfluenceBackupAsync(Uri destinationDirectory, DateTime backupDate)
        {
            Condition.Requires(destinationDirectory, "DestinationDirectory").IsNotNull();

            var filename = String.Format("Confluence-backup-{0}.zip", backupDate.ToString("yyyyMMdd"));
            var url = String.Format("https://{0}/webdav/backupmanager/{1}", InstanceAddress.Host, filename);

            var destinationFilepath = Path.Combine(destinationDirectory.ToString(), filename);
            
            await DownloadBackupAsync(new Uri(url), new Uri(destinationFilepath));
        }

        public async Task DownloadJiraBackupAsync(Uri destinationDirectory, DateTime backupDate)
        {
            Condition.Requires(destinationDirectory, "DestinationDirectory").IsNotNull();

            var filename = String.Format("JIRA-backup-{0}.zip", backupDate.ToString("yyyyMMdd"));
            var url = String.Format("https://{0}/webdav/backupmanager/{1}", InstanceAddress.Host, filename);

            var destinationFilepath = Path.Combine(destinationDirectory.ToString(), filename);
            await DownloadBackupAsync(new Uri(url), new Uri(destinationFilepath));
        }

        public async Task RequestConfluenceBackupAsync()
        {
            var url = String.Format("https://{0}/wiki/rest/obm/1.0/runbackup", InstanceAddress.Host);

            var requestContent = JsonConvert.SerializeObject(new
            {
                cbAttachments = "true"
            });


            logger.Information("Requesting backup of Confluence.");
            await RequestBackupAsync(new Uri(url), requestContent);
        }

        public async Task RequestJiraBackupAsync()
        {
            var url = String.Format("https://{0}/rest/obm/1.0/runbackup", InstanceAddress.Host);

            var requestContent = JsonConvert.SerializeObject(new
            {
            });

            logger.Information("Requesting backup of JIRA.");
            await RequestBackupAsync(new Uri(url), requestContent);
        }

        private HttpClient BuildHttpClient()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = Cookies ?? new CookieContainer(),
                Credentials = Credentials,
                //Proxy = new WebProxy("http://127.0.0.1:8888"),
                //UseProxy = true,
            };

            return new HttpClient(handler);
        }

        private async Task DownloadBackupAsync(Uri url, Uri destinationFilepath)
        {
            Condition.Requires(url, "url").IsNotNull().Evaluate(x => x.IsAbsoluteUri);
            Condition.Requires(destinationFilepath, "destinationFilepath").IsNotNull().Evaluate(x => x.IsFile);

            try
            {
                using (logger.BeginTimedOperation(url.ToString(), "Download", LogEventLevel.Information))
                {
                    using (var client = BuildHttpClient())
                    {
                        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                        response.EnsureSuccessStatusCode();

                        using (var fileStream = File.Create(destinationFilepath.LocalPath))
                        {
                            using (var httpStream = await response.Content.ReadAsStreamAsync())
                            {
                                httpStream.CopyTo(fileStream);
                                fileStream.Flush();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to download {url}: {ex}", url, ex);
            }
        }

        private async Task RequestBackupAsync(Uri url, string requestContent)
        {
            Condition.Requires(url, "Uri").IsNotNull();

            logger.Debug("{@requestContent} has been sent to {url}", requestContent, url);

            using (var client = BuildHttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");

                client.DefaultRequestHeaders.Add("X-Atlassian-Token", "no-check");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");


                var request = new StringContent(requestContent)
                {
                    Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
                };


                var response = await client.PostAsync(url, request);
                var responseContent = await response.Content.ReadAsStringAsync();

                // HTTP 500
                // Backup frequency is limited. You can not make another backup right now. Approximate time till next allowed backup: 47h 50m
                if (responseContent.Contains("Backup frequency is limited"))
                {
                    logger.Warning(responseContent);
                }
                else
                {
                    response.EnsureSuccessStatusCode();

                    logger.Information("Backup request successful.");
                }

            }
        }
    }
}