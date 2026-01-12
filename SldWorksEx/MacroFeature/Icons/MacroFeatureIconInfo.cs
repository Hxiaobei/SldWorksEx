using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using CodeStack.SwEx.MacroFeature.Attributes;
using System;
using System.Drawing;
using System.IO;

namespace CodeStack.SwEx.MacroFeature.Icons {
    internal static class MacroFeatureIconInfo {
        internal const string DEFAULT_ICON_FOLDER = "CodeStack\\SwEx.MacroFeature\\{0}\\Icons";
        internal const string RegularName = "Regular";
        internal const string SuppressedName = "Suppressed";
        internal const string HighlightedName = "Highlighted";

        internal static readonly Size Size = new Size(16, 18);
        internal static readonly Size SizeHighResSmall = new Size(20, 20);
        internal static readonly Size SizeHighResMedium = new Size(32, 32);
        internal static readonly Size SizeHighResLarge = new Size(40, 40);

        private static readonly string[] IconNames = { RegularName, SuppressedName, HighlightedName };

        internal static string GetLocation(Type macroFeatType) {
            if(!macroFeatType.TryGetAttribute<FeatureIconAttribute>(out var attr) 
                ||string.IsNullOrEmpty(attr.IconFolderName)) {

                var folder = string.Format(DEFAULT_ICON_FOLDER, macroFeatType.FullName);
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), folder);
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                attr.IconFolderName);
        }

        internal static string[] GetIcons(Type macroFeatType, bool highRes) {
            var loc = GetLocation(macroFeatType);

            Size[] sizes = highRes
                ? new[] { SizeHighResSmall, SizeHighResMedium, SizeHighResLarge }
                : new[] { Size };

            var result = new string[IconNames.Length * sizes.Length];
            int idx = 0;

            foreach(var size in sizes) 
                foreach(var name in IconNames) 
                    result[idx++] = Path.Combine(loc, IconSizeInfo.CreateFileName(name, size));

            return result;
        }
    }
}
