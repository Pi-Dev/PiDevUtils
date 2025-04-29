using System.Globalization;
using System.Text;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Utilities for working with Unicode text, especially "fancy" or stylized mathematical characters.
 * Provides normalization to ASCII, identification of stylized letters, and surrogate pair handling.
 * Useful for cleaning up user input, chat systems, or data normalization in multilingual contexts.
 *
 * ============= Usage =============
 * string clean = someString.NormalizeLeetText();
 * bool isFancy = Utils.IsMathematicalLetter(codePoint);
 * char ascii = Utils.ConvertToAsciiEquivalent(codePoint);
 */

namespace PiDev
{
    public static class UnicodeUtils
    {
        // Set of Unicode tools for dealing with fancy letters and symbols.
        static int GetUnicodeCodePoint(string input, ref int index)
        {
            char highSurrogate = input[index];

            // Check if the character is a high surrogate
            if (char.IsHighSurrogate(highSurrogate) && index + 1 < input.Length)
            {
                char lowSurrogate = input[index + 1];
                if (char.IsLowSurrogate(lowSurrogate))
                {
                    // Combine high and low surrogate to get the full Unicode code point
                    index++; // Advance the index since we consumed two characters
                    return 0x10000 + ((highSurrogate - 0xD800) << 10) + (lowSurrogate - 0xDC00);
                }
            }

            // If it's not a surrogate pair, just return the character itself
            return highSurrogate;
        }

        public static string NormalizeLeetText(this string input)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                int codePoint = GetUnicodeCodePoint(input, ref i);

                // Step 1: Check if it's a non-Latin script (like Japanese/Chinese), keep it.
                if (IsMathematicalLetter(codePoint))
                {
                    // Step 2: If code point is a mathematical fancy letter, convert to ASCII.
                    result.Append(ConvertToAsciiEquivalent(codePoint));
                }
                else if (char.GetUnicodeCategory((char)codePoint) == UnicodeCategory.OtherLetter)
                {
                    result.Append(char.ConvertFromUtf32(codePoint));
                }
                else
                {
                    // Step 3: Default to just appending the character as it is.
                    result.Append(char.ConvertFromUtf32(codePoint));
                }
            }

            return result.ToString();
        }

        // Check if a rune is in the Mathematical Alphanumeric Symbols block
        public static bool IsMathematicalLetter(int c)
        {
            // Unicode ranges for fancy "mathematical letters"
            // Using integer values instead of character literals for Unicode code points
            return (c >= 0x1D400 && c <= 0x1D7FF);
        }

        // Convert to printable equivalent
        public static char ConvertToAsciiEquivalent(int c)
        {
            // Bold A-Z (𝐀 - 𝐙)
            if (c >= 0x1D400 && c <= 0x1D419) return (char)(c - 0x1D400 + 'A');
            // Bold a-z (𝐚 - 𝐳)
            if (c >= 0x1D41A && c <= 0x1D433) return (char)(c - 0x1D41A + 'a');
            // Italic A-Z (𝐴 - 𝑍)
            if (c >= 0x1D434 && c <= 0x1D44D) return (char)(c - 0x1D434 + 'A');
            // Italic a-z (𝑎 - 𝑧)
            if (c >= 0x1D44E && c <= 0x1D467) return (char)(c - 0x1D44E + 'a');
            // Bold Italic A-Z (𝑨 - 𝒁)
            if (c >= 0x1D468 && c <= 0x1D481) return (char)(c - 0x1D468 + 'A');
            // Bold Italic a-z (𝒂 - 𝒛)
            if (c >= 0x1D482 && c <= 0x1D49B) return (char)(c - 0x1D482 + 'a');
            // Script A-Z (𝒜 - 𝒵)
            if (c >= 0x1D49C && c <= 0x1D4B5) return (char)(c - 0x1D49C + 'A');
            // Script a-z (𝒶 - 𝓏)
            if (c >= 0x1D4B6 && c <= 0x1D4CF) return (char)(c - 0x1D4B6 + 'a');
            // Bold Script A-Z (𝓐 - 𝓩)
            if (c >= 0x1D4D0 && c <= 0x1D4E9) return (char)(c - 0x1D4D0 + 'A');
            // Bold Script a-z (𝓪 - 𝔃)
            if (c >= 0x1D4EA && c <= 0x1D503) return (char)(c - 0x1D4EA + 'a');
            // Fraktur A-Z (𝔄 - 𝔜)
            if (c >= 0x1D504 && c <= 0x1D51C) return (char)(c - 0x1D504 + 'A');
            // Fraktur a-z (𝔞 - 𝔷)
            if (c >= 0x1D51E && c <= 0x1D537) return (char)(c - 0x1D51E + 'a');
            // Double-struck A-Z (𝔸 - 𝕐)
            if (c >= 0x1D538 && c <= 0x1D551) return (char)(c - 0x1D538 + 'A');
            // Double-struck a-z (𝕒 - 𝕫)
            if (c >= 0x1D552 && c <= 0x1D56B) return (char)(c - 0x1D552 + 'a');
            // Bold Fraktur A-Z (𝕬 - 𝖅)
            if (c >= 0x1D56C && c <= 0x1D585) return (char)(c - 0x1D56C + 'A');
            // Bold Fraktur a-z (𝖆 - 𝖟)
            if (c >= 0x1D586 && c <= 0x1D59F) return (char)(c - 0x1D586 + 'a');
            // Sans-serif A-Z (𝖠 - 𝖹)
            if (c >= 0x1D5A0 && c <= 0x1D5B9) return (char)(c - 0x1D5A0 + 'A');
            // Sans-serif a-z (𝖺 - 𝗓)
            if (c >= 0x1D5BA && c <= 0x1D5D3) return (char)(c - 0x1D5BA + 'a');
            // Sans-serif Bold A-Z (𝗔 - 𝗭)
            if (c >= 0x1D5D4 && c <= 0x1D5ED) return (char)(c - 0x1D5D4 + 'A');
            // Sans-serif Bold a-z (𝗮 - 𝘇)
            if (c >= 0x1D5EE && c <= 0x1D607) return (char)(c - 0x1D5EE + 'a');
            // Sans-serif Italic A-Z (𝘈 - 𝘡)
            if (c >= 0x1D608 && c <= 0x1D621) return (char)(c - 0x1D608 + 'A');
            // Sans-serif Italic a-z (𝘢 - 𝘻)
            if (c >= 0x1D622 && c <= 0x1D63B) return (char)(c - 0x1D622 + 'a');
            // Sans-serif Bold Italic A-Z (𝘼 - 𝙕)
            if (c >= 0x1D63C && c <= 0x1D655) return (char)(c - 0x1D63C + 'A');
            // Sans-serif Bold Italic a-z (𝙖 - 𝙯)
            if (c >= 0x1D656 && c <= 0x1D66F) return (char)(c - 0x1D656 + 'a');
            // Monospace A-Z (𝙰 - 𝚉)
            if (c >= 0x1D670 && c <= 0x1D689) return (char)(c - 0x1D670 + 'A');
            // Monospace a-z (𝚊 - 𝚣)
            if (c >= 0x1D68A && c <= 0x1D6A3) return (char)(c - 0x1D68A + 'a');
            // Monospace 0-9 (𝟶 - 𝟿)
            if (c >= 0x1D7F6 && c <= 0x1D7FF) return (char)(c - 0x1D7F6 + '0');

            // Default return the original character if no mapping exists
            return (char)c;
        }
    }
}