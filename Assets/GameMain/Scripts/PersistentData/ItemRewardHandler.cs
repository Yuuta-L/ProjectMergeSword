using System;
using System.Globalization;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    /// <summary>
    /// 道具奖励：<c>Kind = "Item"</c>（取自 <see cref="WealthTypes.Item"/> 的枚举名），
    /// <c>Id</c> 是 <see cref="EnumItem"/> 的数值，数量是 <c>Count</c>。
    /// <para>
    /// 标了 <see cref="RewardHandlerAttribute"/>，不需要在任何启动流程注册；<c>TypeCode</c> 直接用 <see cref="WealthTypes"/>，
    /// 兑换码等服务端协议里的数字类型码天然对得上，工程侧不再写 int → Kind 的映射。
    /// </para>
    /// <para>DSL 示例：<c>Item:1001:1000</c>（1001 = 金币 x1000）。</para>
    /// </summary>
    [RewardHandler(TypeCode = WealthTypes.Item)]
    public sealed class ItemRewardHandler : IRewardHandler
    {
        public const string KindName = nameof(WealthTypes.Item);

        public string Kind => KindName;

        public bool CanGrant(RewardSpec spec, out string reason)
        {
            if (!TryParse(spec, out _, out reason))
            {
                return false;
            }

            if (Burnicle.Persistent == null)
            {
                reason = "PersistentComponent 未就绪";
                return false;
            }

            return true;
        }

        public bool Grant(RewardSpec spec)
        {
            if (!TryParse(spec, out var item, out var reason))
            {
                Log.Warning($"[Reward] 道具奖励无效：{reason}");
                return false;
            }

            var component = Burnicle.Persistent;
            if (component == null)
            {
                Log.Error("[Reward] PersistentComponent 未就绪，道具未发放");
                return false;
            }

            return component.AddItem(item, (int)spec.Count);
        }

        public void Undo(RewardSpec spec)
        {
            if (!TryParse(spec, out var item, out var reason))
            {
                Log.Error($"[Reward] 回滚道具失败：{reason}");
                return;
            }

            var component = Burnicle.Persistent;
            if (component == null || !component.RemoveItem(item, (int)spec.Count))
            {
                Log.Error($"[Reward] 回滚道具失败：{item} x{spec.Count}");
            }
        }

        /// <summary>Id 必须是已定义的 <see cref="EnumItem"/>，数量必须落在 int 范围内。</summary>
        private static bool TryParse(RewardSpec spec, out EnumItem item, out string reason)
        {
            item = EnumItem.None;
            reason = null;

            if (!int.TryParse(spec.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ||
                !Enum.IsDefined(typeof(EnumItem), id))
            {
                reason = $"Id 不是有效的 EnumItem：{spec.Id}";
                return false;
            }

            if (spec.Count <= 0 || spec.Count > int.MaxValue)
            {
                reason = $"数量越界：{spec.Count}";
                return false;
            }

            item = (EnumItem)id;
            return true;
        }
    }
}
