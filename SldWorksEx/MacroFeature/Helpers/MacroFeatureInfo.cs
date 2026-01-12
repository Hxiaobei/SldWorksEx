//**********************
//SwEx.MacroFeature - framework for developing macro features in SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/macro-feature
//**********************

using CodeStack.SwEx.Common.Reflection;
using CodeStack.SwEx.MacroFeature.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeStack.SwEx.MacroFeature.Helpers {
    internal static class MacroFeatureInfo {
        internal static string GetBaseName<TMacroFeature>() where TMacroFeature : MacroFeatureEx {
            return GetBaseName(typeof(TMacroFeature));
        }

        internal static string GetBaseName(Type macroFeatType) {
            if(!typeof(MacroFeatureEx).IsAssignableFrom(macroFeatType))
                throw new InvalidCastException(
                    $"{macroFeatType.FullName} must inherit {typeof(MacroFeatureEx).FullName}");

            if(macroFeatType.TryGetAttribute<OptionsAttribute>(out var opt)
                && !string.IsNullOrEmpty(opt.BaseName)) {
                return opt.BaseName;
            }

            if(macroFeatType.TryGetAttribute<DisplayNameAttribute>(out var dis)
                && !string.IsNullOrEmpty(dis.DisplayName)) {
                return dis.DisplayName;
            }

            return macroFeatType.Name;
        }


        internal static string GetProgId<TMacroFeature>() where TMacroFeature : MacroFeatureEx
            => GetProgId(typeof(TMacroFeature));


        internal static string GetProgId(Type macroFeatType) {
            if(!typeof(MacroFeatureEx).IsAssignableFrom(macroFeatType))
                throw new InvalidCastException( $"{macroFeatType.FullName} must inherit {typeof(MacroFeatureEx).FullName}");

            return macroFeatType.TryGetAttribute<ProgIdAttribute>(out var pro)
                && !string.IsNullOrEmpty(pro.Value)
            ? pro.Value
            : macroFeatType.FullName;
        }

    }
}
