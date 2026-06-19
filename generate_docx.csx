#!/usr/bin/env dotnet-script
// Simple script to create an HTML file that Word can open as DOCX
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

string basePath = @"c:\Users\ofrig\source\repos\dirhb\EXERCISES";
string inputFile = Path.Combine(basePath, "ספר_פרויקט_מלא.md");
string outputFile = Path.Combine(basePath, "ספר_פרויקט_JobFindUltimate.html");

string md = File.ReadAllText(inputFile, Encoding.UTF8);

// Very simple Markdown to HTML conversion
string html = ConvertMarkdownToHtml(md);

string fullHtml = $@"<!DOCTYPE html>
<html dir=""rtl"" lang=""he"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>ספר פרויקט - JobFindUltimate</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Heebo:wght@400;600;700&display=swap');
  body {{
    font-family: 'Heebo', Arial, sans-serif;
    direction: rtl;
    text-align: right;
    margin: 60px auto;
    max-width: 900px;
    color: #222;
    line-height: 1.8;
    font-size: 12pt;
  }}
  h1 {{ font-size: 24pt; color: #1a237e; border-bottom: 3px solid #1a237e; padding-bottom: 8px; page-break-before: always; }}
  h1:first-of-type {{ page-break-before: avoid; }}
  h2 {{ font-size: 18pt; color: #283593; border-bottom: 1px solid #283593; padding-bottom: 4px; margin-top: 30px; }}
  h3 {{ font-size: 14pt; color: #303f9f; margin-top: 20px; }}
  h4 {{ font-size: 12pt; color: #3949ab; margin-top: 15px; }}
  p {{ margin: 10px 0; }}
  table {{ border-collapse: collapse; width: 100%; margin: 20px 0; }}
  th {{ background-color: #283593; color: white; padding: 8px 12px; text-align: right; }}
  td {{ border: 1px solid #ccc; padding: 8px 12px; }}
  tr:nth-child(even) {{ background-color: #e8eaf6; }}
  code {{ background: #f5f5f5; padding: 2px 6px; border-radius: 3px; font-family: Consolas, monospace; direction: ltr; display: inline-block; }}
  pre {{ background: #1e1e1e; color: #d4d4d4; padding: 16px; border-radius: 6px; overflow-x: auto; direction: ltr; text-align: left; font-family: Consolas, monospace; font-size: 10pt; line-height: 1.5; }}
  pre code {{ background: none; padding: 0; color: inherit; }}
  blockquote {{ border-right: 4px solid #283593; margin-right: 0; padding-right: 16px; color: #555; background: #e8eaf6; padding: 12px 16px; }}
  ul, ol {{ padding-right: 30px; padding-left: 0; }}
  li {{ margin: 4px 0; }}
  strong {{ color: #1a237e; }}
  .cover {{
    text-align: center;
    margin-bottom: 80px;
    padding: 40px;
    border: 3px solid #1a237e;
    border-radius: 10px;
    background: linear-gradient(135deg, #e8eaf6, #c5cae9);
  }}
  hr {{ border: none; border-top: 2px solid #283593; margin: 30px 0; }}
  @media print {{
    pre {{ white-space: pre-wrap; }}
    h1 {{ page-break-before: always; }}
    h1:first-of-type {{ page-break-before: avoid; }}
  }}
</style>
</head>
<body>
{html}
</body>
</html>";

File.WriteAllText(outputFile, fullHtml, Encoding.UTF8);
Console.WriteLine($"Done! Saved to: {outputFile}");

string ConvertMarkdownToHtml(string markdown)
{
    var sb = new StringBuilder();
    var lines = markdown.Split('\n');
    bool inCodeBlock = false;
    bool inTable = false;
    bool inList = false;
    string codeBlockLang = "";

    foreach (var rawLine in lines)
    {
        string line = rawLine.TrimEnd('\r');
        
        // Code block toggle
        if (line.StartsWith("```"))
        {
            if (!inCodeBlock)
            {
                inCodeBlock = true;
                codeBlockLang = line.Substring(3).Trim();
                if (inList) { sb.AppendLine("</ul>"); inList = false; }
                sb.AppendLine($"<pre><code class=\"language-{codeBlockLang}\">");
            }
            else
            {
                inCodeBlock = false;
                sb.AppendLine("</code></pre>");
            }
            continue;
        }
        
        if (inCodeBlock)
        {
            sb.AppendLine(EscapeHtml(line));
            continue;
        }

        // Table detection
        if (line.Contains("|") && line.Trim().StartsWith("|"))
        {
            if (!inTable)
            {
                if (inList) { sb.AppendLine("</ul>"); inList = false; }
                inTable = true;
                sb.AppendLine("<table>");
                var headers = ParseTableRow(line);
                sb.AppendLine("<tr>");
                foreach (var h in headers)
                    sb.AppendLine($"<th>{h}</th>");
                sb.AppendLine("</tr>");
            }
            else if (line.Contains("---"))
            {
                // skip separator
            }
            else
            {
                var cells = ParseTableRow(line);
                sb.AppendLine("<tr>");
                foreach (var c in cells)
                    sb.AppendLine($"<td>{ApplyInlineMarkdown(c)}</td>");
                sb.AppendLine("</tr>");
            }
            continue;
        }
        else if (inTable)
        {
            sb.AppendLine("</table>");
            inTable = false;
        }

        // HR
        if (line.Trim() == "---")
        {
            if (inList) { sb.AppendLine("</ul>"); inList = false; }
            sb.AppendLine("<hr/>");
            continue;
        }

        // Headings
        if (line.StartsWith("# ")) { if (inList) { sb.AppendLine("</ul>"); inList = false; } sb.AppendLine($"<h1>{ApplyInlineMarkdown(line.Substring(2))}</h1>"); continue; }
        if (line.StartsWith("## ")) { if (inList) { sb.AppendLine("</ul>"); inList = false; } sb.AppendLine($"<h2>{ApplyInlineMarkdown(line.Substring(3))}</h2>"); continue; }
        if (line.StartsWith("### ")) { if (inList) { sb.AppendLine("</ul>"); inList = false; } sb.AppendLine($"<h3>{ApplyInlineMarkdown(line.Substring(4))}</h3>"); continue; }
        if (line.StartsWith("#### ")) { if (inList) { sb.AppendLine("</ul>"); inList = false; } sb.AppendLine($"<h4>{ApplyInlineMarkdown(line.Substring(5))}</h4>"); continue; }

        // List items
        if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
        {
            if (!inList) { sb.AppendLine("<ul>"); inList = true; }
            string item = line.TrimStart().Substring(2);
            sb.AppendLine($"<li>{ApplyInlineMarkdown(item)}</li>");
            continue;
        }
        else if (inList && !string.IsNullOrWhiteSpace(line))
        {
            sb.AppendLine("</ul>");
            inList = false;
        }

        // Blockquote
        if (line.StartsWith("> "))
        {
            if (inList) { sb.AppendLine("</ul>"); inList = false; }
            sb.AppendLine($"<blockquote>{ApplyInlineMarkdown(line.Substring(2))}</blockquote>");
            continue;
        }

        // Empty line
        if (string.IsNullOrWhiteSpace(line))
        {
            if (inList) { sb.AppendLine("</ul>"); inList = false; }
            sb.AppendLine("<br/>");
            continue;
        }

        // Paragraph
        sb.AppendLine($"<p>{ApplyInlineMarkdown(line)}</p>");
    }
    
    if (inList) sb.AppendLine("</ul>");
    if (inTable) sb.AppendLine("</table>");
    
    return sb.ToString();
}

string[] ParseTableRow(string line)
{
    var parts = line.Trim().Trim('|').Split('|');
    for (int i = 0; i < parts.Length; i++)
        parts[i] = parts[i].Trim();
    return parts;
}

string EscapeHtml(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

string ApplyInlineMarkdown(string text)
{
    // Bold
    text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
    text = Regex.Replace(text, @"__(.+?)__", "<strong>$1</strong>");
    // Italic
    text = Regex.Replace(text, @"\*(.+?)\*", "<em>$1</em>");
    // Inline code
    text = Regex.Replace(text, @"`(.+?)`", "<code>$1</code>");
    // Links
    text = Regex.Replace(text, @"\[(.+?)\]\(.+?\)", "$1");
    return text;
}
