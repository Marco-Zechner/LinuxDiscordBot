namespace ConsoleDiscordBot
{
    public class CodeBoxDrawer
    {
        static readonly char[] thinBox = ['┌', '─', '┐', '│', '┘', '└'];
        static readonly char[] thickBox = ['╔', '═', '╗', '║', '╝', '╚'];
        static readonly char[] connectionChar = ['┤', '├', '╡', '╞', '╢', '╟', '╣', '╠'];
        public static string DrawBox(string content, int padding = 1, bool thick = false)
        {
            if (padding < 0)
                padding = 0;

            char[] boxChars = thinBox;
            if (thick)
                boxChars = thickBox;

            string boxSizedContent = PadContentLeft(content);

            int widthPadding = 1 + padding * 2;
            int boxWidthWithoutBorder = boxSizedContent.Split('\n')[0].Length + widthPadding * 2;

            string box = boxChars[0] + new string(boxChars[1], boxWidthWithoutBorder) + boxChars[2] + "\n";

            for (int i = 0; i < padding; i++)
            {
                box += boxChars[3] + new string(' ', boxWidthWithoutBorder) + boxChars[3] + "\n";
            }

            foreach (var line in boxSizedContent.Split('\n'))
            {
                box += boxChars[3] + new string(' ', widthPadding) + line + new string(' ', widthPadding) + boxChars[3] + "\n";
            }

            for (int i = 0; i < padding; i++)
            {
                box += boxChars[3] + new string(' ', boxWidthWithoutBorder) + boxChars[3] + "\n";
            }

            box += boxChars[5] + new string(boxChars[1], boxWidthWithoutBorder) + boxChars[4] + "\n";
            box += new string(' ', boxWidthWithoutBorder + 3);

            return box;
        }

        public static string ConnectBoxes(string box1, string box2, float alignmentPercentLeft = 0.5f)
        {
            alignmentPercentLeft = Math.Clamp(alignmentPercentLeft, 0, 1);

            string topBox = box1;
            string bottomBox = box2;

            int topBoxWidth = topBox.Split('\n')[0].Length;
            int bottomBoxWidth = bottomBox.Split('\n')[0].Length;

            if (bottomBoxWidth < topBoxWidth)
            {
                (bottomBox, topBox) = (topBox, bottomBox);
                (bottomBoxWidth, topBoxWidth) = (topBoxWidth, bottomBoxWidth);
            }

            int horizontalCharsToFill = bottomBoxWidth - (2 + topBoxWidth);
            int charsToFillLeft = (int)(horizontalCharsToFill * alignmentPercentLeft);

            if (horizontalCharsToFill == -2)
            {
                List<string> topBoxLines = [.. topBox.Split('\n')];
                topBoxLines.RemoveAt(topBoxLines.Count - 2);

                List<string> bottomBoxLines = [.. bottomBox.Split('\n')];
                bottomBoxLines.RemoveAt(0);

                return $"{string.Join('\n', topBoxLines)}{string.Join('\n', bottomBoxLines)}";
            }

            if (horizontalCharsToFill == -1)
            {
                return DrawBox("Only 1 character difference is not possible", 0, true);
            }

            char[] bottomBoxChars = thinBox;

            bool topBoxThick = topBox[0] == thickBox[0];
            int connectionCharOffset = 0;
            if (topBoxThick)
            {
                connectionCharOffset += 4;
            }

            bool bottomBoxThick = bottomBox[0] == thickBox[0];
            if (bottomBoxThick)
            {
                connectionCharOffset += 2;
                bottomBoxChars = thickBox;
            }

            string connectedBoxes = ".";

            string[] topLines = topBox.Split('\n');
            for (int i = 0; i < topLines.Length - 3; i++)
            {
                connectedBoxes += new string(' ', charsToFillLeft) + topLines[i] + new string(' ', horizontalCharsToFill - charsToFillLeft + 1) + "\n";
            }

            string[] bottomLines = bottomBox.Split('\n');

            connectedBoxes += bottomBoxChars[0]
                + new string(bottomBoxChars[1], charsToFillLeft)
                + connectionChar[0 + connectionCharOffset]
                + topLines[^3][1..^1]
                + connectionChar[1 + connectionCharOffset]
                + new string(bottomBoxChars[1], horizontalCharsToFill - charsToFillLeft) 
                + bottomBoxChars[2] + "\n";

            connectedBoxes += bottomBoxChars[3]
                + new string(' ', charsToFillLeft)
                + topLines[^2]
                + new string(' ', horizontalCharsToFill - charsToFillLeft)
                + bottomBoxChars[3] + "\n";

            for (int i = 1; i < bottomLines.Length - 1; i++)
            {
                connectedBoxes += bottomLines[i] + "\n";
            }

            return connectedBoxes;
        }

        public static string DrawBoxWithHeader(string header, string content, float alignmentPercentLeft = 0.5f, int headerPadding = 0, int contentPadding = 1, bool thickHeader = true, bool thickContent = false)
        {
            string headerBox = DrawBox(header, headerPadding, thickHeader);
            string contentBox = DrawBox(content, contentPadding, thickContent);

            return ConnectBoxes(headerBox, contentBox, alignmentPercentLeft);
        }

        public static string PadContentLeft(string content)
        {
            int maxLineLength = content.Split('\n').Max(x => x.Length);
            string paddedContent = "";
            foreach (var line in content.Split('\n'))
            {
                paddedContent += line.PadRight(maxLineLength) + "\n";
            }
            return paddedContent.TrimEnd('\n');
        }
    }
}
