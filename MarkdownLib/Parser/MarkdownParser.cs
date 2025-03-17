using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownLib.Parser
{
    /// <summary>
    /// Provides functionality for parsing Markdown formatted text into HTML.
    /// </summary>
    public class MarkdownParser : IMarkdownParser
    {
        /// <summary>
        /// Parses a Markdown formatted string and converts it to HTML.
        /// </summary>
        /// <param name="markdown">The Markdown formatted text.</param>
        /// <returns>A HTML string representing the parsed Markdown content.</returns>
        public string Parse(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            // Split content into lines
            var lines = markdown.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            var sb = new StringBuilder();

            // Block states
            bool inCodeBlock = false;
            string codeBlockLanguage = "";
            var codeBlockContent = new StringBuilder();

            bool inList = false;
            string listType = ""; // "ul" or "ol"
            var listItems = new List<string>();

            var paragraphLines = new List<string>();
            var blockquoteLines = new List<string>();

            // Local methods to flush pending blocks
            void FlushParagraph()
            {
                if (paragraphLines.Count > 0)
                {
                    string paragraph = string.Join(" ", paragraphLines);
                    sb.AppendFormat("<p>{0}</p>", paragraph);
                    paragraphLines.Clear();
                }
            }

            void FlushList()
            {
                if (inList && listItems.Count > 0)
                {
                    if (listType == "ul")
                        sb.Append("<ul>");
                    else if (listType == "ol")
                        sb.Append("<ol>");

                    foreach (var item in listItems)
                    {
                        sb.AppendFormat("<li>{0}</li>", item);
                    }

                    if (listType == "ul")
                        sb.Append("</ul>");
                    else if (listType == "ol")
                        sb.Append("</ol>");

                    listItems.Clear();
                    inList = false;
                    listType = "";
                }
            }

            void FlushBlockquote()
            {
                if (blockquoteLines.Count > 0)
                {
                    // Join blockquote lines separated by <br/>
                    string content = string.Join("<br/>", blockquoteLines);
                    sb.AppendFormat("<blockquote>{0}</blockquote>", content);
                    blockquoteLines.Clear();
                }
            }

            // Process lines one by one
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Check start/end of fenced code block
                if (line.StartsWith("```"))
                {
                    FlushParagraph();
                    FlushList();
                    FlushBlockquote();

                    if (!inCodeBlock)
                    {
                        inCodeBlock = true;
                        codeBlockLanguage = line.Length > 3 ? line.Substring(3).Trim() : "";
                        codeBlockContent.Clear();
                    }
                    else
                    {
                        // End code block
                        string languageClass = string.IsNullOrEmpty(codeBlockLanguage) ? "" : $" class=\"language-{codeBlockLanguage}\"";
                        sb.AppendFormat("<pre><code{0}>{1}</code></pre>",
                            languageClass,
                            WebUtility.HtmlEncode(codeBlockContent.ToString()));
                        inCodeBlock = false;
                        codeBlockLanguage = "";
                        codeBlockContent.Clear();
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeBlockContent.AppendLine(line);
                    continue;
                }

                // Blank line: flush pending blocks
                if (string.IsNullOrWhiteSpace(line))
                {
                    FlushParagraph();
                    FlushList();
                    FlushBlockquote();
                    continue;
                }

                // Blockquote (lines starting with '>')
                if (line.TrimStart().StartsWith(">"))
                {
                    FlushParagraph();
                    FlushList();
                    // Remove '>' and any following space
                    string content = line.TrimStart().TrimStart('>', ' ');
                    blockquoteLines.Add(ProcessInline(content));
                    continue;
                }
                else if (blockquoteLines.Count > 0)
                {
                    // If the line is not a blockquote, flush the accumulated blockquote
                    FlushBlockquote();
                }

                // Unordered list items (starting with -, + or *)
                var ulMatch = Regex.Match(line, @"^\s*([-+*])\s+(.*)$");
                if (ulMatch.Success)
                {
                    FlushParagraph();
                    if (!inList || listType != "ul")
                    {
                        FlushList();
                        inList = true;
                        listType = "ul";
                    }
                    listItems.Add(ProcessInline(ulMatch.Groups[2].Value.Trim()));
                    continue;
                }

                // Ordered list items (starting with digits followed by a period)
                var olMatch = Regex.Match(line, @"^\s*\d+\.\s+(.*)$");
                if (olMatch.Success)
                {
                    FlushParagraph();
                    if (!inList || listType != "ol")
                    {
                        FlushList();
                        inList = true;
                        listType = "ol";
                    }
                    listItems.Add(ProcessInline(olMatch.Groups[1].Value.Trim()));
                    continue;
                }

                // Headings – from level 1 to 6 (starting with #)
                var headingMatch = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
                if (headingMatch.Success)
                {
                    FlushParagraph();
                    FlushList();
                    FlushBlockquote();
                    int level = headingMatch.Groups[1].Value.Length;
                    string content = ProcessInline(headingMatch.Groups[2].Value.Trim());
                    sb.AppendFormat("<h{0}>{1}</h{0}>", level, content);
                    continue;
                }

                // Table detection logic
                if (line.Contains("|"))
                {
                    FlushParagraph();
                    FlushList();
                    FlushBlockquote();

                    var tableLines = new List<string> { line };

                    // Capture all lines belonging to the table
                    while (i + 1 < lines.Length && lines[i + 1].Contains("|"))
                    {
                        tableLines.Add(lines[++i]);
                    }

                    sb.Append(ProcessTable(tableLines));
                    continue;
                }

                // Horizontal rule (at least 3 -, * or _ characters)
                if (Regex.IsMatch(line.Trim(), @"^([-*_]\s*){3,}$"))
                {
                    FlushParagraph();
                    FlushList();
                    FlushBlockquote();
                    sb.Append("<hr />");
                    continue;
                }

                // Default case: accumulate as paragraph content
                paragraphLines.Add(ProcessInline(line.Trim()));
            }

            // Flush any blocks that are still open
            FlushParagraph();
            FlushList();
            FlushBlockquote();

            return $"<div id='markdown-note'>{sb.ToString()}</div>";
        }

        /// <summary>
        /// Processes inline elements: images, links, inline code, bold and italic text.
        /// </summary>
        /// <param name="text">The text containing inline Markdown syntax.</param>
        /// <returns>The text with inline Markdown syntax converted to HTML.</returns>
        private string ProcessInline(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Process image: ![alt](url)
            text = Regex.Replace(text, @"!\[([^\]]*)\]\(([^)]+)\)", "<img src=\"$2\" alt=\"$1\" />");

            // Process link: [text](url)
            text = Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "<a href=\"$2\">$1</a>");

            // Process inline code: `code`
            text = Regex.Replace(text, @"`([^`]+)`", "<code>$1</code>");

            // Process emphasis (order is important)
            // First, mixed emphasis: ***text***
            text = Regex.Replace(text, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
            // Bold: **text**
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            // Italic: *text*
            text = Regex.Replace(text, @"\*(.+?)\*", "<em>$1</em>");

            return text;
        }

        /// <summary>
        /// Processes a Markdown table and converts it to HTML table format.
        /// </summary>
        /// <param name="tableLines">A list of strings representing the lines of a Markdown table.</param>
        /// <returns>A HTML string representing the parsed table.</returns>
        private string ProcessTable(List<string> tableLines)
        {
            if (tableLines.Count < 2)
                return string.Join("\n", tableLines);

            var headers = tableLines[0].Split('|').Select(h => h.Trim()).ToArray();
            var aligns = tableLines[1].Split('|').Select(col =>
            {
                col = col.Trim();
                if (Regex.IsMatch(col, @"^:\-+:$")) return "center";
                if (Regex.IsMatch(col, @"^\-+:$")) return "right";
                return "left";
            }).ToArray();

            var sb = new StringBuilder("<table><thead><tr>");
            for (int i = 0; i < headers.Length; i++)
            {
                sb.AppendFormat("<th style='text-align:{0}'>{1}</th>", aligns[i], headers[i]);
            }
            sb.Append("</tr></thead><tbody>");

            foreach (var rowLine in tableLines.Skip(2))
            {
                var cols = rowLine.Split('|').Select(c => c.Trim()).ToArray();
                sb.Append("<tr>");
                for (int i = 0; i < cols.Length; i++)
                {
                    sb.AppendFormat("<td style='text-align:{0}'>{1}</td>", aligns.ElementAtOrDefault(i) ?? "left", cols[i]);
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }
    }
}
