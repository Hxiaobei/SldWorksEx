using System.Collections.Generic;
using CodeStack.SwEx.MacroFeature.Attributes;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Msg.SwMsgTs.Features.Cube {
    /// <summary>
    /// 正方体宏特征的参数数据模型
    /// </summary>
    public class CubeData {
        /// <summary>
        /// 选择的平面实体（IFace2 或参考平面）
        /// </summary>
        [ParameterSelection]
        public IFace2 SelectedFace { get; set; }

        /// <summary>
        /// 正方体边长（线性尺寸，可在图形区域中拖动修改）
        /// </summary>
        [ParameterDimension(swDimensionType_e.swLinearDimension)]
        public double SideLength { get; set; } = 0.05; // 默认 50mm
    }
}
