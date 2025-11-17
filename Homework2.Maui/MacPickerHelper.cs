using Homework2.Maui.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homework2.Maui.Helpers
{
    public static class MacPickerHelper
    {
        /// <summary>
        /// Displays a selection dialog that works well on Mac
        /// </summary>
        public static async Task<T?> DisplayPickerAsync<T>(
            ContentPage page,
            string title,
            IEnumerable<T> items,
            Func<T, string> displayFunc,
            T? currentSelection = default)
        {
            if (items == null || !items.Any())
            {
                await page.DisplayAlert("No Options", "No items available to select.", "OK");
                return default;
            }

            var itemsList = items.ToList();
            var displayNames = itemsList.Select(displayFunc).ToArray();
            
            string action = await page.DisplayActionSheet(
                title,
                "Cancel",
                null,
                displayNames
            );

            if (action == "Cancel" || string.IsNullOrEmpty(action))
                return default;

            var index = Array.IndexOf(displayNames, action);
            if (index >= 0 && index < itemsList.Count)
                return itemsList[index];

            return default;
        }

        /// <summary>
        /// Displays a selection dialog for sorting options
        /// </summary>
        public static async Task<int> DisplaySortPickerAsync(
            ContentPage page,
            string title,
            string[] options)
        {
            if (options == null || options.Length == 0)
                return -1;

            string action = await page.DisplayActionSheet(
                title,
                "Cancel",
                null,
                options
            );

            if (action == "Cancel" || string.IsNullOrEmpty(action))
                return -1;

            return Array.IndexOf(options, action);
        }
    }
}