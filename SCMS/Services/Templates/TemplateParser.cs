using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SCMS.Services.Template
{
    public class TemplateParser
    {
        private const int MaxRecursionDepth = 25;

        public string Parse(string template, Dictionary<string, object> context)
        {
            // Strip all HTML comments globally before parsing
            template = Regex.Replace(template, @"<!--.*?-->", "", RegexOptions.Singleline);
            return Parse(template, context, 0);
        }

        private string Parse(string template, object model, int depth)
        {
            if (depth > MaxRecursionDepth || string.IsNullOrWhiteSpace(template) || model == null)
                return template;

            var output = new StringBuilder();
            for (int i = 0; i < template.Length;)
            {
                // Skip over HTML comments entirely
                if (template.Substring(i).StartsWith("<!--"))
                {
                    int commentEnd = template.IndexOf("-->", i + 4);
                    if (commentEnd == -1) break; // Malformed comment, exit early
                    output.Append(template.Substring(i, commentEnd + 3 - i));
                    i = commentEnd + 3;
                    continue;
                }

                if (MatchAt(template, i, "{{#each ", out _))
                {
                    int start = i + 8;
                    int end = template.IndexOf("}}", start);
                    var collectionName = template.Substring(start, end - start).Trim();

                    int blockStart = end + 2;
                    int blockEnd = FindBlockEnd(template, blockStart, "each");
                    var blockContent = template.Substring(blockStart, blockEnd - blockStart);

                    var prop = ResolveProperty(model, collectionName);
                    var items = prop as IEnumerable;

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            output.Append(Parse(blockContent, item, depth + 1));
                        }
                    }

                    i = blockEnd + "{{/each}}".Length;
                }
                else if (MatchAt(template, i, "{{#if ", out _))
                {
                    int start = i + 6;
                    int end = template.IndexOf("}}", start);
                    var propName = template.Substring(start, end - start).Trim();

                    int blockStart = end + 2;
                    int blockEnd = FindBlockEnd(template, blockStart, "if");
                    string trueBlock, falseBlock = "";

                    int elseIndex = FindElse(template, blockStart, blockEnd);
                    if (elseIndex != -1)
                    {
                        trueBlock = template.Substring(blockStart, elseIndex - blockStart);
                        falseBlock = template.Substring(elseIndex + 8, blockEnd - (elseIndex + 8));
                    }
                    else
                    {
                        trueBlock = template.Substring(blockStart, blockEnd - blockStart);
                    }

                    var value = ResolveProperty(model, propName);

                    bool condition = value switch
                    {
                        null => false,
                        bool b => b,
                        int iVal => iVal != 0,
                        string s => !string.IsNullOrWhiteSpace(s),
                        IEnumerable e => e.Cast<object>().Any(),
                        _ => true
                    };

                    output.Append(Parse(condition ? trueBlock : falseBlock, model, depth + 1));

                    i = blockEnd + "{{/if}}".Length;
                }
                else if (MatchAt(template, i, "{{", out _))
                {
                    int end = template.IndexOf("}}", i + 2);
                    var key = template.Substring(i + 2, end - i - 2).Trim();
                    var value = ResolveProperty(model, key);
                    output.Append(value?.ToString() ?? "");

                    i = end + 2;
                }
                else
                {
                    output.Append(template[i]);
                    i++;
                }
            }

            return output.ToString();
        }

        private static bool MatchAt(string src, int index, string match, out int matchStart)
        {
            matchStart = index;
            return src.Length >= index + match.Length && src.Substring(index, match.Length) == match;
        }

        private static int FindBlockEnd(string template, int start, string type)
        {
            int nesting = 1;
            int i = start;

            while (i < template.Length)
            {
                if (template.Substring(i).StartsWith("{{#" + type))
                    nesting++;
                else if (template.Substring(i).StartsWith("{{/" + type + "}}"))
                    nesting--;

                if (nesting == 0)
                    return i;

                i++;
            }

            var snippet = template.Substring(start, Math.Min(200, template.Length - start));
            throw new Exception($"Unmatched {{#{type}}} block near: {snippet}");
        }

        private static int FindElse(string template, int start, int end)
        {
            int i = start;
            while (i < end - 8)
            {
                if (template.Substring(i, 8) == "{{else}}")
                    return i;
                i++;
            }
            return -1;
        }

        private static object? ResolveProperty(object model, string key)
        {
            if (model is Dictionary<string, object> dict)
                return dict.TryGetValue(key, out var val) ? val : null;

            var prop = model.GetType().GetProperty(key);
            return prop?.GetValue(model);
        }
    }
}
