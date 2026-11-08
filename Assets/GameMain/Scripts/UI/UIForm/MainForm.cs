using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{
    /// <summary>
    /// 模板主界面：只负责展示，业务从 OnOpen 开始接入。
    /// </summary>
    public class MainForm : UIFormBase
    {
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Log.Info("[MergeSword] 进入主界面");
        }
    }
}
