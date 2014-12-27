using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using CommandLine;
using CommandLine.Text;

namespace AtlassianCloudBackup
{
    public class CommandLineOptions
    {
        [ParserState]
        public IParserState LastParserState { get; set; }

        [Option('d', "destination", Required = true,
            HelpText = "Destination directory where backups will be written to.")]
        public string DestinationPath { get; set; }


        [Option('s', "sleep", Required = true,
            HelpText =
                "Amount of time in seconds to sleep after reqeusting a backup before download is started. Increase if your instance is large."
            )]
        public int Sleep { get; set; }


        [Option('i', "instance", Required = true, HelpText = "Atlassian Cloud instance url.")]
        public string Url { get; set; }


        [Option('u', "username", Required = true, HelpText = "Atlassian Cloud username with administrative privledges.")
        ]
        public string Username { get; set; }


        [Option('p', "password", Required = true, HelpText = "Password for administrative account.")]
        public string Password { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            string processname = Process.GetCurrentProcess().ProcessName;

            var help = new HelpText
            {
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true,
                Copyright = new CopyrightInfo("Geoffrey Huntley <ghuntley@ghuntley.com>", DateTime.Now.Year),
                MaximumDisplayWidth = 160,
            };

            help.AddPreOptionsLine(Environment.NewLine);
            help.AddPreOptionsLine(
                String.Format(
                    "Usage: {0} --destination C:\\backups\\ --sleep 600 --instance https://yourinstance.atlassian.net --username admin --password password",
                    processname));

            help.AddOptions(this);

            if (LastParserState.Errors.Count <= 0) return help;

            string errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
            if (!string.IsNullOrEmpty(errors))
            {
                help.AddPostOptionsLine(Environment.NewLine);
                help.AddPostOptionsLine("ERROR(s):");help.AddPostOptionsLine(Environment.NewLine);
                help.AddPostOptionsLine(errors);
            }

            return help;
        }
    }
}