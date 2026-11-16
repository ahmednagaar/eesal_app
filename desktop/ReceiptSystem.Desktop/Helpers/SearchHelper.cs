using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ReceiptSystem.Desktop.Helpers;

/// <summary>
/// Shared search helper for multi-word partial matching.
/// Splits the user's input into tokens and checks that every token
/// matches the beginning of at least one word in the target name.
/// Includes Arabic letter normalization (alef variants, taa marbuta, hamza).
/// </summary>
public static class SearchHelper
{
    /// <summary>
    /// Returns true when every space-separated token in <paramref name="query"/>
    /// is a prefix of at least one word in <paramref name="name"/>.
    /// Matching is case-insensitive and Arabic-normalized.
    /// </summary>
    /// <example>
    /// MatchesMultiWord("وائل فاروق الدكه", "وا فارو الدك") → true
    /// MatchesMultiWord("مغربي سلوى", "مغرب") → true
    /// </example>
    public static bool MatchesMultiWord(string name, string query)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(query))
            return false;

        var normalizedName = NormalizeArabic(name);
        var normalizedQuery = NormalizeArabic(query);

        var queryTokens = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var nameWords = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (queryTokens.Length == 0)
            return false;

        // Every query token must be a prefix of at least one name word
        foreach (var token in queryTokens)
        {
            bool found = false;
            foreach (var word in nameWords)
            {
                if (word.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes Arabic text for search purposes:
    /// - Normalizes alef variants (أ إ آ ٱ) → ا
    /// - Normalizes taa marbuta (ة) → ه
    /// - Removes Arabic diacritics (tashkeel: fatha, damma, kasra, sukun, shadda, tanwin)
    /// - Removes tatweel/kashida (ـ)
    /// </summary>
    private static string NormalizeArabic(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var chars = new char[text.Length];
        int pos = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Skip Arabic diacritics (tashkeel)
            if (c is '\u064B'  // fathatan
                  or '\u064C'  // dammatan
                  or '\u064D'  // kasratan
                  or '\u064E'  // fatha
                  or '\u064F'  // damma
                  or '\u0650'  // kasra
                  or '\u0651'  // shadda
                  or '\u0652'  // sukun
                  or '\u0653'  // maddah above
                  or '\u0654'  // hamza above
                  or '\u0655') // hamza below
                continue;

            // Skip tatweel/kashida
            if (c == '\u0640')
                continue;

            // Normalize alef variants → ا
            if (c is '\u0623'  // أ (alef with hamza above)
                  or '\u0625'  // إ (alef with hamza below)
                  or '\u0622'  // آ (alef with madda)
                  or '\u0671') // ٱ (alef wasla)
            {
                chars[pos++] = '\u0627'; // ا
                continue;
            }

            // Normalize taa marbuta → ه
            if (c == '\u0629') // ة
            {
                chars[pos++] = '\u0647'; // ه
                continue;
            }

            chars[pos++] = c;
        }

        return new string(chars, 0, pos);
    }

    // Highlight brush — accent blue to show the keyboard-focused row
    private static readonly Brush HighlightBrush = new SolidColorBrush(Color.FromArgb(255, 99, 102, 241));   // indigo-500
    private static readonly Brush HighlightFg    = Brushes.White;

    /// <summary>
    /// Attaches keyboard navigation (↑/↓/Enter/Escape) to a search TextBox
    /// with an associated ListBox inside a Popup.
    /// Call this once during page initialization for each search field.
    ///
    /// Design: During ↑/↓ navigation we do NOT use ListBox.SelectedIndex or
    /// ListBoxItem.IsSelected at all — both fire ListBox.SelectionChanged which
    /// causes page handlers to replace ItemsSource and trigger crashes.
    /// Instead we paint the highlighted row with a background brush directly on
    /// the ListBoxItem's visual properties and store the logical index ourselves.
    ///
    /// Enter key commits via the onSelect callback (not SelectionChanged).
    /// Mouse clicks still work normally through SelectionChanged.
    /// </summary>
    public static void AttachKeyboardNavigation(
        TextBox searchBox,
        ListBox listBox,
        Popup popup,
        Action<object> onSelect)
    {
        int highlightIndex = -1;

        // Reset highlight whenever the text changes (new search = new list)
        searchBox.TextChanged += (s, e) =>
        {
            // Clear visual highlight on old list before ItemsSource is replaced
            ClearHighlight(listBox, highlightIndex);
            highlightIndex = -1;
        };

        searchBox.PreviewKeyDown += (s, e) =>
        {
            if (!popup.IsOpen) return;

            // Always re-read count — ItemsSource may have been replaced since last keystroke.
            int count = listBox.Items.Count;
            if (count == 0) return;

            switch (e.Key)
            {
                case Key.Down:
                    e.Handled = true;
                    ClearHighlight(listBox, highlightIndex);
                    highlightIndex = (highlightIndex + 1 < count) ? highlightIndex + 1 : 0;
                    if (highlightIndex >= listBox.Items.Count) highlightIndex = 0;
                    ApplyHighlight(listBox, highlightIndex);
                    break;

                case Key.Up:
                    e.Handled = true;
                    ClearHighlight(listBox, highlightIndex);
                    highlightIndex = (highlightIndex - 1 >= 0) ? highlightIndex - 1 : count - 1;
                    if (highlightIndex >= listBox.Items.Count) highlightIndex = listBox.Items.Count - 1;
                    if (highlightIndex < 0) highlightIndex = 0;
                    ApplyHighlight(listBox, highlightIndex);
                    break;

                case Key.Enter:
                    e.Handled = true;
                    // Re-read count at moment of access — always use live count for Items[].
                    int liveCount = listBox.Items.Count;
                    if (highlightIndex >= 0 && highlightIndex < liveCount)
                    {
                        ClearHighlight(listBox, highlightIndex);
                        onSelect(listBox.Items[highlightIndex]);
                        highlightIndex = -1;
                    }
                    else if (liveCount == 1)
                    {
                        // Auto-select when there is only one result
                        ClearHighlight(listBox, 0);
                        onSelect(listBox.Items[0]);
                        highlightIndex = -1;
                    }
                    break;

                case Key.Escape:
                    e.Handled = true;
                    popup.IsOpen = false;
                    ClearHighlight(listBox, highlightIndex);
                    highlightIndex = -1;
                    break;
            }
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Highlight helpers — paint background directly on the ListBoxItem
    // WITHOUT touching IsSelected or SelectedIndex, so that
    // ListBox.SelectionChanged is NEVER fired during ↑/↓ navigation.
    // ─────────────────────────────────────────────────────────────

    /// <summary>Paints the highlight brush on the item at <paramref name="index"/> and scrolls it into view.</summary>
    private static void ApplyHighlight(ListBox listBox, int index)
    {
        try
        {
            if (index < 0 || index >= listBox.Items.Count) return;

            // Scroll first so the container gets generated
            listBox.ScrollIntoView(listBox.Items[index]);

            var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            if (container != null)
            {
                // Paint highlight visually — do NOT touch IsSelected
                container.Background = HighlightBrush;
                container.Foreground = HighlightFg;
            }
            // If container is not yet realized, skip — it will be re-highlighted
            // on the next frame via ScrollIntoView which forces virtualization to realize it.
        }
        catch (ArgumentOutOfRangeException) { /* ItemsSource replaced mid-operation — ignore */ }
    }

    /// <summary>Removes the highlight brush from item at <paramref name="index"/>.</summary>
    private static void ClearHighlight(ListBox listBox, int index)
    {
        try
        {
            if (index < 0 || index >= listBox.Items.Count) return;
            var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            if (container != null)
            {
                // Restore to transparent so the ListBox style takes over again
                container.Background = Brushes.Transparent;
                container.ClearValue(ListBoxItem.ForegroundProperty);
            }
        }
        catch (ArgumentOutOfRangeException) { /* ignore */ }
    }
}
