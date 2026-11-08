using System.Collections.Generic;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    public class BaseData : PersistentData
    {
        /// <summary>
        /// �ϴμ��ÿ��ˢ�µ�ʱ��
        /// </summary>
        public long LastDailyRefreshTime;
        /// <summary>
        /// ÿ�����õ�boolֵ
        /// </summary>
        public Dictionary<string, bool> DailyBoolValues = new Dictionary<string, bool>();
        /// <summary>
        /// ���ô��ڵ�boolֵ
        /// </summary>
        public Dictionary<string, bool> ConstBoolValues = new Dictionary<string, bool>();
        /// <summary>
        /// ÿ�����õ�intֵ
        /// </summary>
        public Dictionary<string, int> DailyIntValues = new Dictionary<string, int>();
        /// <summary>
        /// ���ô��ڵ�intֵ
        /// </summary>
        public Dictionary<string, int> ConstIntValues = new Dictionary<string, int>();
    }
}