using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using CommandLine;
using AtlassianCloudBackupClient;
using Serilog;
using Serilog.Events;

namespace AtlassianCloudBackup
{
    internal class Program
    {
        private static ILogger logger;

        /// <summary>
        ///     The exit/return code (aka %ERRORLEVEL%) on application exit.
        /// </summary>
        public enum ExitCode
        {
            Success = 0,
            Error = 1
        }

        private static void Main(string[] args)
        {
            logger = new LoggerConfiguration()
                .ReadAppSettings()
                .WriteTo.RollingFile(@"logs\{Date}.txt", LogEventLevel.Verbose)
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Logger = logger;

            Task t = MainAsync(args);
            t.Wait();
        }
        private static async Task MainAsync(string[] args)
        {
            var options = new CommandLineOptions();


            // Parse in 'strict mode'; i.e. success or quit
            if (Parser.Default.ParseArgumentsStrict(args, options))
            {
                try
                {
                    Log.Debug("Results of parsing command line arguments: {@options}", options);

                    var credentials = new NetworkCredential()
                    {
                        UserName = options.Username,
                        Password = options.Password
                    };

                    var backupClient = new BackupClient(new Uri(options.Url), credentials);

                    logger.Information("Started backup.");

                    await backupClient.AuthenticateAsync();

                    await backupClient.RequestJiraBackupAsync();
                    await backupClient.RequestConfluenceBackupAsync();

                    logger.Information("Sleeping for {seconds} seconds before starting download(s).", options.Sleep);
                    Thread.Sleep(options.Sleep*1000);
                    await backupClient.DownloadJiraBackupAsync(new Uri(options.DestinationPath), DateTime.Now);
                    await backupClient.DownloadConfluenceBackupAsync(new Uri(options.DestinationPath), DateTime.Now);

                    logger.Information("Finished backup.");
                    Environment.Exit((int) ExitCode.Success);
                }
                catch (Exception ex)
                {
                    Log.Fatal("A fatal error occurred: {ex}", ex);
                }
            }

            Environment.Exit((int) ExitCode.Error);
        }
    }
}
