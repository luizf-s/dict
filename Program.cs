using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;

using System.Reflection;

using HtmlAgilityPack;
// Parse do HTML para uma representacao interna (object/struct)
// Imprimir na tela

namespace dict
{
    class Definition
    {
        public string WordClass { get; init; }
        public IList<string> Definitions { get; init; }
    }

    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            try
            {
                string word = args[0];
                string definitionHtml = GetHtmlPageForWord(word);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(definitionHtml);
                IList<Definition> defs = GetDefinitions(htmlDoc);
                foreach (var d in defs)
                {
                    PrintDefinition(d);
                    Console.WriteLine();
                }
            }
            catch (IndexOutOfRangeException error)
            {
                Console.WriteLine($"Please insert a word. {0}", error.Message);
                Environment.Exit(1);
            }
        }

        static IList<Definition> GetDefinitions(HtmlDocument htmlDoc)
        {
            var titleNode = GetTitleNode(htmlDoc);
            var definitions = new List<Definition>();

            for (var definition = titleNode.NextSibling; definition != null; definition = definition.NextSibling)
            {
                var wordClass = definition.FirstChild.InnerText;
                var def = new Definition
                {
                    WordClass = wordClass,
                    Definitions = GetExpressions(definition.FirstChild.NextSibling),
                };
                definitions.Add(def);
            }

            return definitions;
        }

        static IList<string> GetExpressions(HtmlNode definitionNode)
        {
            IList<string> expressions = new List<string>();
            for (var current = definitionNode.FirstChild; current != null; current = current.NextSibling)
            {
                if (current.Name != "div")
                    continue;

                for (var i = current.FirstChild; i != null; i = i.NextSibling)
                {
                    if(IsDefSpan(i))
                        expressions.Add(ParseText(i.InnerText));
                    if (i.Name == "div")
                        for (var def = i.FirstChild; def != null; def = def.NextSibling)
                            expressions.Add(ParseText(def.InnerText));
                }
            }

            return expressions;
        }

        static string ParseText(string rawText)
        {
            return rawText;
        }

        static bool IsDefSpan(HtmlNode node)
        {
            const string defSpanClass = "one-click-content css-nnyc96 e1q3nk1v1";
            return node.Name == "span" && node.Attributes["class"].Value == defSpanClass;
        }

        static void PrintChildren(HtmlNode node)
        {
            for (var c = node.FirstChild; c != null; c = c.NextSibling)
                PrintNode(c, $"=== children of {node.Name} ===\n");
        }

        static void PrintNode(HtmlNode node, string prefix = "")
        {
            Console.Write("{0}{1}", prefix, node.Name);
            if (node.Attributes.Contains("class"))
                Console.Write(" - {0} -", node.Attributes["class"].Value);
            Console.WriteLine(" `{0}...`", node.InnerText.Substring(0, 10));
        }

        static HtmlNode GetTitleNode(HtmlDocument htmlDoc)
        {
            return htmlDoc
                .DocumentNode
                .SelectSingleNode("//section[@class='entry-headword']");
        }

        static void PrintDefinition(Definition definition)
        {
            Console.WriteLine("  {0}", definition.WordClass);
            foreach (var (def, index) in definition.Definitions.Select((value, index) => (value, index)))
            {
                // tupla (tuple)
                // def = ("a definição", 0)
                // def = ("a definição", 1)
                // def = ("a definição", 2)
                // def = ("a definição", 3)
                Console.WriteLine("    {0}) {1}", index + 1, def);
            }
        }

        static string GetHtmlPageForWord(string word)
        {
            try
            {
                return client.GetStringAsync($"https://www.dictionary.com/browse/{word}").Result;
            }
            catch (AggregateException error)
            {
                Console.WriteLine($"Word {word} does not exist! {error.Message}");
                Environment.Exit(0);
            }
            return "";
        }
    }
}