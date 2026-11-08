using BurnicleFramework.Runtime;
using GameFramework;

namespace MergeSword
{
    public class EntityBase : EntityLogic
    {
        public int SerialId { get; private set; }
        protected EntityParams Params;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (userData == null)
            {
                Log.Error("创建Entity失败! 你必须为Entity传入EntityParams数据");
            }

            Params = userData as EntityParams;
            SerialId = this.Entity.SerialId;
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            SerialId = this.Entity.SerialId;
            Params = userData as EntityParams;
            if (userData == null || Params == null)
            {
                Log.Error("创建Entity失败! 你必须为Entity传入EntityParams数据");
                return;
            }

            if (Burnicle.Entity.IsValidEntity(Params.AttachToEntity))
            {
                Burnicle.Entity.AttachEntity(this.Entity, Params.AttachToEntity, Params.ParentTransform);
            }

            if (Params.position != null)
            {
                this.CachedTransform.position = Params.position.Value;
            }

            if (Params.eulerAngles != null)
            {
                this.CachedTransform.eulerAngles = Params.eulerAngles.Value;
            }

            if (Params.localScale != null)
            {
                this.CachedTransform.localScale = Params.localScale.Value;
            }

            if (Params.gameObjectLayer >= 0)
            {
                gameObject.layer = Params.gameObjectLayer;
                //gameObject.SetLayerRecursively(Params.gameObjectLayer);
            }

            Params.OnShowCallback?.Invoke(this);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            Params.OnHideCallback?.Invoke(this);
            base.OnHide(isShutdown, userData);
            if (!isShutdown && Params != null)
            {
                ReferencePool.Release(Params);
            }
        }
    }
}