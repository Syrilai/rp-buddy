using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RpBuddy.Utils
{
    internal abstract class MacroToken
    {
        public abstract string ToMacroString();
    }

    internal class TextToken : MacroToken
    {
        public string Text { get; set; }
        public TextToken(string text)
        {
            Text = text;
        }

        public override string ToMacroString()
        {
            return Text;
        }
    }

    internal class MacroTagToken : MacroToken
    {
        public string Tag { get; set; }

        public MacroTagToken(string tag)
        {
            Tag = tag;
        }

        public override string ToMacroString()
        {
            return $"<{Tag}>";
        }
    }

    internal class TextPosition
    {
        public int TokenIndex { get; set; }
        public int CharIndexInToken { get; set; }
    }

    internal enum MatchType
    {
        Quote,
        Action,
        OOC,
        Continued,
        Done
    }

    internal class ChatParser
    {
        private readonly IPluginLog _log;

        public ChatParser(IPluginLog log)
        {
            _log = log;
        }

        public List<MacroToken> Tokenize(string macroCode)
        {
            var tokens = new List<MacroToken>();
            var i = 0;
            var textBuffer = new StringBuilder();

            while (i < macroCode.Length)
            {
                if (macroCode[i] == '<')
                {
                    if (textBuffer.Length > 0)
                    {
                        tokens.Add(new TextToken(textBuffer.ToString()));
                        textBuffer.Clear();
                    }

                    var endIdx = macroCode.IndexOf('>', i);
                    if (endIdx == -1)
                    {
                        textBuffer.Append(macroCode[i]);
                        i++;
                        continue;
                    }

                    var tag = macroCode.Substring(i + 1, endIdx - i - 1);
                    tokens.Add(new MacroTagToken(tag));
                    i = endIdx + 1;
                }
                else
                {
                    textBuffer.Append(macroCode[i]);
                    i++;
                }
            }

            if (textBuffer.Length > 0)
            {
                tokens.Add(new TextToken(textBuffer.ToString()));
            }

            return tokens;
        }

        private (string TextOnly, List<TextPosition> PositionMap) ExtractTextWithMapping(List<MacroToken> tokens)
        {
            var textOnly = new StringBuilder();
            var positionMap = new List<TextPosition>();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is TextToken textToken)
                {
                    for (int charIdx = 0; charIdx < textToken.Text.Length; charIdx++)
                    {
                        positionMap.Add(new TextPosition { TokenIndex = i, CharIndexInToken = charIdx });
                        textOnly.Append(textToken.Text[charIdx]);
                    }
                }
            }

            return (textOnly.ToString(), positionMap);
        }

        private List<(int StartPos, int EndPos, MatchType Type)> FindRpMatches(string textOnly)
        {
            var matches = new List<(int StartPos, int EndPos, MatchType Type)>();
            var i = 0;
            
            while (i < textOnly.Length)
            {
                var doneMatch = Patterns.Done.Match(textOnly, i);
                if (doneMatch.Success && doneMatch.Index == i)
                {
                    matches.Add((i, i + doneMatch.Length, MatchType.Done));
                    i += doneMatch.Length;
                    continue;
                }

                var continuedMatch = Patterns.Continued.Match(textOnly, i);
                if (continuedMatch.Success && continuedMatch.Index == i)
                {
                    matches.Add((i, i + continuedMatch.Length, MatchType.Continued));
                    i += continuedMatch.Length;
                    continue;
                }

                var textAtPosition = textOnly[i];

                if (textAtPosition == '"')
                {
                    var endQuote = FindClosingQuote(textOnly, i + 1);
                    if (endQuote != -1)
                    {
                        matches.Add((i, endQuote + 1, MatchType.Quote));
                        i = endQuote + 1;
                        continue;
                    }
                }

                if (textAtPosition == '(' || textAtPosition == '[')
                {
                    var oocEnd = FindOocEnd(textOnly, i);
                    if (oocEnd != -1)
                    {
                        matches.Add((i, oocEnd + 1, MatchType.OOC));
                        i = oocEnd + 1;
                        continue;
                    }
                }

                if (textOnly[i] == '*')
                {
                    var endAsterisk = FindClosingAsterisk(textOnly, i + 1);
                    if (endAsterisk != -1)
                    {
                        matches.Add((i, endAsterisk + 1, MatchType.Action));
                        i = endAsterisk + 1;
                        continue;
                    }
                }

                i++;
            }

            return matches;
        }

        private int FindClosingQuote(string text, int start)
        {
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '"')
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindOocEnd(string text, int start)
        {
            var openChar = text[start];
            var closeChar = openChar switch
            {
                '(' => ')',
                '[' => ']',
                _ => '\0'
            };

            if (closeChar == '\0') return -1;

            var isDouble = start + 1 < text.Length && text[start + 1] == openChar;
            var expectedClose = isDouble ? new string(closeChar, 2) : closeChar.ToString();

            if (isDouble)
            {
                for (int i = start + 2; i < text.Length - 1; i++)
                {
                    if (text[i] == closeChar && text[i + 1] == closeChar)
                    {
                        return i + 1;
                    }
                }
            } else
            {
                for (int i = start + 1; i < text.Length; i++)
                {
                    if (text[i] == closeChar)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private int FindClosingAsterisk(string text, int start)
        {
            if (start >= text.Length || char.IsWhiteSpace(text[start]))
            {
                return -1;
            }

            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '*')
                {
                    if (i > start)
                    {
                        return i;
                    }
                    return -1;
                }
            }

            return -1;
        }

        public List<MacroToken> ApplyRpFormatting(List<MacroToken> tokens)
        {
            var (textOnly, positionMap) = ExtractTextWithMapping(tokens);

            if (positionMap.Count == 0)
            {
                return new List<MacroToken>(tokens);
            }

            var matches = FindRpMatches(textOnly);

            if (matches.Count == 0)
            {
                return new List<MacroToken>(tokens);
            }

            var result = new List<MacroToken>();
            var processedTokens = new HashSet<int>();
            var lastTextPos = 0;

            foreach (var match in matches)
            {
                AddTokensBetweenPositions(tokens, positionMap, lastTextPos, match.StartPos, result, processedTokens);

                AddFormattedMatch(tokens, positionMap, match, textOnly, result, processedTokens);

                lastTextPos = match.EndPos;
            }

            AddTokensBetweenPositions(tokens, positionMap, lastTextPos, textOnly.Length, result, processedTokens);

            for (int i = 0; i < tokens.Count; i++)
            {
                if (!processedTokens.Contains(i) && tokens[i] is MacroTagToken)
                {
                    result.Add(tokens[i]);
                }
            }

            return result;
        }

        private void AddTokensBetweenPositions(
            List<MacroToken> tokens,
            List<TextPosition> positionMap,
            int startTextPos,
            int endTextPos,
            List<MacroToken> result,
            HashSet<int> processedTokens)
        {
            if (startTextPos >= endTextPos || startTextPos >= positionMap.Count)
            {
                return;
            }

            var startToken = positionMap[startTextPos];
            var endToken = endTextPos <= positionMap.Count ?
                positionMap[Math.Min(endTextPos, positionMap.Count) - 1] :
                positionMap[positionMap.Count - 1];

            for (int tokenIdx = startToken.TokenIndex; tokenIdx <= endToken.TokenIndex && tokenIdx < tokens.Count; tokenIdx++)
            {
                var token = tokens[tokenIdx];

                if (token is MacroTagToken)
                {
                    result.Add(token);
                    processedTokens.Add(tokenIdx); // Track macro tags
                }
                else if (token is TextToken textToken)
                {
                    var startChar = (tokenIdx == startToken.TokenIndex) ?
                        startToken.CharIndexInToken :
                        0;
                    var endChar = (tokenIdx == endToken.TokenIndex) ?
                        endToken.CharIndexInToken + 1 :
                        textToken.Text.Length;

                    if (startChar < endChar && endChar <= textToken.Text.Length)
                    {
                        result.Add(new TextToken(textToken.Text.Substring(startChar, endChar - startChar)));
                    }
                    processedTokens.Add(tokenIdx); // Track text tokens
                }
            }
        }

        private void AddFormattedMatch(
            List<MacroToken> tokens,
            List<TextPosition> positionMap,
            (int StartPos, int EndPos, MatchType Type) match,
            string textOnly,
            List<MacroToken> result,
            HashSet<int> processedTokens)
        {
            switch (match.Type)
            {
                case MatchType.Quote:
                    result.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorSay})"));
                    AddQuoteWithItalics(tokens, positionMap, match, textOnly, result, processedTokens);
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    break;

                case MatchType.Action:
                    result.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorEmoteUser})"));
                    AddTokensForMatch(tokens, positionMap, match, result, processedTokens);
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    break;

                case MatchType.OOC:
                    result.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorTell})"));
                    AddTokensForMatch(tokens, positionMap, match, result, processedTokens);
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    break;

                case MatchType.Continued:
                    result.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorEcho})"));
                    AddTokensForMatch(tokens, positionMap, match, result, processedTokens);
                    result.Add(new TextToken(" "));
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    result.Add(new MacroTagToken("color(16755477)"));
                    result.Add(new TextToken(""));
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    break;

                case MatchType.Done:
                    result.Add(new MacroTagToken($"color(gnum{(int)GlobalExpressions.ColorEcho})"));
                    AddTokensForMatch(tokens, positionMap, match, result, processedTokens);
                    result.Add(new TextToken(" "));
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    result.Add(new MacroTagToken("color(1703780)"));
                    result.Add(new TextToken("✓"));
                    result.Add(new MacroTagToken("color(stackcolor)"));
                    break;
            }
        }

        private void AddTokensForMatch(
            List<MacroToken> tokens,
            List<TextPosition> positionMap,
            (int StartPos, int EndPos, MatchType Type) match,
            List<MacroToken> result,
            HashSet<int> processedTokens)
        {
            if (match.StartPos >= positionMap.Count)
            {
                return;
            }

            var startToken = positionMap[match.StartPos];
            var endIdx = Math.Min(match.EndPos - 1, positionMap.Count - 1);
            var endToken = positionMap[endIdx];

            for (int tokenIdx = startToken.TokenIndex; tokenIdx <= endToken.TokenIndex && tokenIdx < tokens.Count; tokenIdx++)
            {
                var token = tokens[tokenIdx];

                if (token is MacroTagToken)
                {
                    result.Add(token);
                    processedTokens.Add(tokenIdx); // Track that we've seen this token
                }
                else if (token is TextToken textToken)
                {
                    var startChar = (tokenIdx == startToken.TokenIndex) ?
                        startToken.CharIndexInToken :
                        0;
                    var endChar = (tokenIdx == endToken.TokenIndex) ?
                        endToken.CharIndexInToken + 1 :
                        textToken.Text.Length;

                    if (startChar < endChar && endChar <= textToken.Text.Length)
                    {
                        result.Add(new TextToken(textToken.Text.Substring(startChar, endChar - startChar)));
                    }
                    processedTokens.Add(tokenIdx); // Track that we've seen this token
                }
            }
        }

        private void AddQuoteWithItalics(
            List<MacroToken> tokens,
            List<TextPosition> positionMap,
            (int StartPos, int EndPos, MatchType Type) match,
            string textOnly,
            List<MacroToken> result,
            HashSet<int> processedTokens)
        {
            var quoteStart = match.StartPos + 1;
            var quoteEnd = match.EndPos - 1;

            if (quoteStart >= quoteEnd)
            {
                AddTokensForMatch(tokens, positionMap, match, result, processedTokens);
                return;
            }

            AddTokensForMatch(tokens, positionMap, (match.StartPos, quoteStart, match.Type), result, processedTokens);

            var i = quoteStart;
            while (i < quoteEnd)
            {
                var formatDelim = '\0';

                if (textOnly[i] == '*')
                {
                    formatDelim = '*';
                }
                else if (textOnly[i] == '_' || textOnly[i] == '/')
                {
                    formatDelim = textOnly[i];
                }

                if (formatDelim != '\0')
                {
                    var endDelim = FindFormatEnd(textOnly, i + 1, quoteEnd, formatDelim);
                    if (endDelim != -1)
                    {
                        if (i > quoteStart)
                        {
                            AddTokensForMatch(tokens, positionMap, (quoteStart, i, match.Type), result, processedTokens);
                        }

                        if (formatDelim == '*')
                        {
                            result.Add(new MacroTagToken("edgecolor(gnum13)"));
                            AddTokensForMatch(tokens, positionMap, (i + 1, endDelim, match.Type), result, processedTokens);
                            result.Add(new MacroTagToken("edgecolor(stackcolor)"));
                        }
                        else
                        {
                            result.Add(new MacroTagToken("italic(1)"));
                            AddTokensForMatch(tokens, positionMap, (i + 1, endDelim, match.Type), result, processedTokens);
                            result.Add(new MacroTagToken("italic(0)"));
                        }

                        i = endDelim + 1;
                        quoteStart = i;
                        continue;
                    }
                }

                i++;
            }

            if (quoteStart < quoteEnd)
            {
                AddTokensForMatch(tokens, positionMap, (quoteStart, quoteEnd, match.Type), result, processedTokens);
            }

            AddTokensForMatch(tokens, positionMap, (quoteEnd, match.EndPos, match.Type), result, processedTokens);
        }

        private int FindFormatEnd(string text, int start, int maxEnd, char delimiter)
        {
            if (start >= maxEnd || char.IsWhiteSpace(text[start]))
            {
                return -1;
            }

            for (int i = start; i < maxEnd; i++)
            {
                if (text[i] == delimiter && i > start && !char.IsWhiteSpace(text[i - 1]))
                {
                    return i;
                }
            }

            return -1;
        }

        public string SerializeTokens(List<MacroToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var token in tokens)
            {
                sb.Append(token.ToMacroString());
            }
            return sb.ToString();
        }
    }
}
