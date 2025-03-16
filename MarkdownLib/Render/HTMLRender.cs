namespace MarkdownLib.Render
{
    class HTMLRender : IHTMLRender
    {
        public string Render(string parsedContent)
        {
            return string.IsNullOrWhiteSpace(parsedContent) ? string.Empty : parsedContent;
        }
    }
}
