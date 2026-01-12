using CodeStack.SwEx.Common.Reflection;
using CodeStack.SwEx.MacroFeature.Icons;
using System;
using System.Drawing;

namespace CodeStack.SwEx.MacroFeature.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class IconAttribute : Attribute {
        internal MacroFeatureIcon Regular { get; }
        internal MacroFeatureIcon Suppressed { get; }
        internal MacroFeatureIcon Highlighted { get; }

        internal string IconFolderName { get; }

        public IconAttribute(Type resType, string resName, string iconFolderName = "") {
            IconFolderName = iconFolderName;

            Regular = CreateMaster(resType, resName, MacroFeatureIconInfo.RegularName);
            Suppressed = CreateMaster(resType, resName, MacroFeatureIconInfo.SuppressedName);
            Highlighted = CreateMaster(resType, resName, MacroFeatureIconInfo.HighlightedName);
        }

        public IconAttribute(Type resType, string small, string medium, string large, string iconFolderName = "") {
            IconFolderName = iconFolderName;

            Regular = CreateHighRes(resType, small, medium, large, MacroFeatureIconInfo.RegularName);
            Suppressed = CreateHighRes(resType, small, medium, large, MacroFeatureIconInfo.SuppressedName);
            Highlighted = CreateHighRes(resType, small, medium, large, MacroFeatureIconInfo.HighlightedName);
        }

        private static MasterIcon CreateMaster(Type type, string name, string baseName) {
            return new MasterIcon(baseName) { Icon = ResourceHelper.GetResource<Image>(type, name) };
        }

        private static HighResIcon CreateHighRes(Type type, string small, string medium, string large, string baseName) {
            return new HighResIcon(baseName) {
                Small = ResourceHelper.GetResource<Image>(type, small),
                Medium = ResourceHelper.GetResource<Image>(type, medium),
                Large = ResourceHelper.GetResource<Image>(type, large)
            };
        }
    }
}
