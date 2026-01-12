//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Properties;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using System;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Helpers {
    internal static class DisplayInfoExtractor {
        internal static TIcon ExtractCommandDisplayIcon<TIconAtt, TIcon>(Type type,
            Func<Image, TIcon> masterIconCreator,
            Func<TIconAtt, TIcon> extractIcon, bool useDefault = true)
            where TIconAtt : Attribute
            where TIcon : IIcon {
            // 1. 尝试从 TIconAtt 特性提取
            if(type.TryGetAttribute<TIconAtt>(out var att))
                return extractIcon(att);

            // 2. 尝试从 IconAttribute 提取 Image
            var iconAtt = type.TryGetAttribute<IconAttribute>();
            var masterIcon = iconAtt?.Icon;

            // 3. 没有图标 → 使用默认或返回 default
            if(masterIcon == null) {
                if(!useDefault) return default;
                masterIcon = Resources.default_icon;
            }

            // 4. 转换成 TIcon
            return masterIconCreator(masterIcon);
        }

        internal static TIcon ExtractCommandDisplayIcon<TIconAtt, TIcon>(Enum enumer,
            Func<Image, TIcon> masterIconCreator,
            Func<TIconAtt, TIcon> extractIcon)
            where TIconAtt : Attribute
            where TIcon : IIcon {
            var icon = default(TIcon);

            if(!enumer.TryGetAttribute<TIconAtt>(a => icon = extractIcon.Invoke(a))) {
                var masterIcon = (enumer.TryGetAttribute<IconAttribute>()?.Icon) ?? Resources.default_icon;
                icon = masterIconCreator.Invoke(masterIcon);
            }

            return icon;
        }
    }
}
