using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Remipedia.Modules
{
    public class ML : ModuleBase
    {
        [Command("Dream", RunMode = RunMode.Async)]
        public async Task DreamAsync(string url, int layer = 10, int iters = 10, int range = 1)
        {
            if (layer > 18 || layer < 1)
            {
                await ReplyAsync("Invalid layer argument (must be between 1 and 18)");
                return;
            }

            if (iters > 100 || iters < 0)
            {
                await ReplyAsync("Invalid iters argument (must be between 0 and 100)");
                return;
            }

            if (range > 19 - layer || range < 0)
            {
                await ReplyAsync("Invalid range argument (must be between 1 and 19 - layer)");
                return;
            }

            await LaunchMlCommand("dream", new[] { url },
                "darknet", $"nightmare cfg/vgg-conv.cfg vgg-conv.weights [INPATH] {layer} -iters {iters} - range {range}",
                $"_vgg-conv_{layer}_000000.jpg");
        }

        [Command("Dream", RunMode = RunMode.Async), Priority(1)]
        public async Task DreamAsync(int layer = 10, int iters = 10, int range = 1)
        {
            await DreamAsync(GetAttachmentImage(1).ElementAt(0), layer, iters, range);
        }

        [Command("Edge", RunMode = RunMode.Async)]
        public async Task EdgeAsync(string url, double percentile = 99.7)
        {
            if (percentile >= 100 || percentile <= 0)
            {
                await ReplyAsync("Invalid range argument (must be between 0 and 100)");
                return;
            }
            await LaunchMlCommand("edge", new[] { url }, "python", $"sobel.py -I [INPATH] -p {percentile}", ".jpg");
        }

        [Command("Edge", RunMode = RunMode.Async), Priority(1)]
        public async Task EdgeAsync(double percentile = 99.7)
        {
            await EdgeAsync(GetAttachmentImage(1).ElementAt(0), percentile);
        }

        [Command("Transfer", RunMode = RunMode.Async)]
        public async Task TransferAsync(string url1, string url2)
        {
            await LaunchMlCommand("transfer", new[] { url1, url2 }, "python", "PROGRAMNAME.py -I [INPATH]", ".jpg");
        }

        [Command("Transfer", RunMode = RunMode.Async), Priority(1)]
        public async Task TransferAsync()
        {
            var images = GetAttachmentImage(2);
            await TransferAsync(images.ElementAt(0), images.ElementAt(1));
        }

        /// <summary>
        /// Start a machine learning process
        /// </summary>
        /// <param name="discordCmdName">Name of the current Discord command</param>
        /// <param name="url">URL to the image to treat</param>
        /// <param name="command">Command name</param>
        /// <param name="arguments">Command arguments</param>
        /// <param name="outpathEnd">End of the path for the output file, will be append to the current generated file name</param>
        private async Task LaunchMlCommand(string discordCmdName, string[] url, string command, string arguments, string outpathEnd)
        {
            // In and out paths the image will have
            string[] inPaths = new string[url.Length];
            string[] outPaths = new string[url.Length];
            var tmpPath = DateTime.Now.ToString("HHmmssff") + Context.User.Id + "_" + discordCmdName.ToLowerInvariant();

            for (int i = 0; i < url.Length; i++)
            {

                // Check if URL is a valid image
                var extension = Path.GetExtension(url[i]).Split('?')[0];
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                {
                    throw new ArgumentException("Invalid file type " + extension);
                }

                inPaths[i] = $"Inputs/{tmpPath}_{i}{extension}";
                outPaths[i] = tmpPath + "_" + i + outpathEnd;

                // Download the image
                var bytes = await StaticObjects.HttpClient.GetByteArrayAsync(url[i]);
                if (bytes.Length > 8_000_000)
                {
                    throw new ArgumentException("Your image must be less than 8MB");
                }
                File.WriteAllBytes(inPaths[i], bytes);
            }

            // Replace [INPATH] string in the command by the actual path
            arguments = arguments.Replace("[INPATH]", string.Join(" ", inPaths));
            try
            {
                var msg = await ReplyAsync("Your image is processed, this can take up to a few minutes");
                Console.WriteLine(new LogMessage(LogSeverity.Info, discordCmdName, command + " " + arguments));
                Process.Start(command, arguments).WaitForExit();

                // When we are done, we delete the waiting message and post the modified image
                foreach (var oPath in outPaths)
                {
                    await Context.Channel.SendFileAsync(oPath);
                }
                await msg.DeleteAsync();

                DeleteFiles(inPaths);
                DeleteFiles(outPaths);
            }
            catch (Exception)
            {
                DeleteFiles(inPaths);
                DeleteFiles(outPaths);
                throw;
            }
        }

        public void DeleteFiles(string[] path)
        {
            foreach (var p in path)
            {
                File.Delete(p);
            }
        }

        /// <summary>
        /// Get the file in attachment or throw an error if there are none
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetAttachmentImage(int count)
        {
            if (Context.Message.Attachments.Count < count)
            {
                throw new ArgumentException($"You must provide at least {count} image");
            }
            return Enumerable.Range(0, count)
                .Select(x => Context.Message.Attachments.ElementAt(x).Url);
        }
    }
}
