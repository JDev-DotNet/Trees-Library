using System;
using System.Collections.Generic;
using System.Text;

namespace JDev.Trees
{
    /// <summary>
    /// Kiểu duyệt cây
    /// </summary>
    public enum VisitMode
    {
        /// <summary>
        /// Duyệt trái - gốc - phải
        /// </summary>
        LeftRootRight = 1,
        
        /// <summary>
        /// Duyệt phải - gốc trái
        /// </summary>
        RightRootLeft = 2,

        /// <summary>
        /// Duyệt gốc - trái - phải
        /// </summary>
        RootLeftRight = 3,
    }
}
