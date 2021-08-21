using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Remipedia.Modules
{
    public class Information : ModuleBase
    {
        [Command("Help")]
        public async Task HelpAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = new Color(0x067FBD),
                Title = "Help",
                Fields = new()
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Dream [url to image] [layer] [iters] [range]",
                        Value = @"Dream use VGG-16 as a backbone so only 18 layers are usable.\n
[layer] optional (1 - 19)(default to 10) choose the layer you want to use for gradient maximization.
Low level (near 0) will show basics changes (colors, straight lines, etc...), whereas high level (near 16) will show more animal structures.\n

[iters] optional (1 - 100)(default to 10). Number of iterations to apply to the image, the more iterations, the more feverish the DREAM will be!\n

[range] optional (1 - 19-layer)(default to 1). Number of layers after the selected layer (with layer argument) that will be choosen at random for each iteration.
Large number will makes the DREAM more diverse in the range of possibles transformations."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Edge [url to image] [percentile]",
                        Value = "todo"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Transfer [url to image] [url to image]",
                        Value = "Transfer the style of the first image into the second one and vice versa"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "PCA [url to image] [color pattern]",
                        Value = "todo"
                    }
                }
            }.Build());
        }
    }
}
