using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{
    public partial class Burnicle : MonoBehaviour
    {
        void Start()
        {
            RegisterBuiltinComponents();
            RegisterCustomComponents();
            BurnicleEntry.InitComponents();
        }
    }
}
