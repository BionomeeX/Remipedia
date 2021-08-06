using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Remipedia.TypeReader
{
    public sealed class ColorStringReader : Discord.Commands.TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (input.Length != 3)
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, "You must provide a string with 3 letters, containing either R, G or B");
            }
            input = input.ToUpperInvariant();
            var first = GetColorValue(input[0]);
            var second = GetColorValue(input[1]);
            var third = GetColorValue(input[2]);

            if (!first.HasValue || !second.HasValue || !third.HasValue
                && first != second && second != first && first != third)
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, "You must provide a string with 3 letters, containing either R, G or B");
            }

            return TypeReaderResult.FromSuccess(new ColorString
            {
                First = first.Value,
                Second = second.Value,
                Third = third.Value
            });
        }

        public ColorString.ColorValue? GetColorValue(char c)
        {
            return c switch
            {
                'R' => ColorString.ColorValue.R,
                'B' => ColorString.ColorValue.B,
                'G' => ColorString.ColorValue.G,
                _ => null
            };
        }
    }
}
