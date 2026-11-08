using System.Collections.Generic;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    public class PurchaseHelper
    {
        private static ProductList _productList;
        
        public static void RequestProductInfo()
        {
            if (_productList != null)
            {
                return;
            }
            
            Burnicle.CrossPlatform.RequestProductInfo(list =>
            {
                _productList = list;
                if (list == null)
                {
                    Log.Warning("获取商品列表失败");
                }
            });
        }
        
        public static BurnicleFramework.Runtime.ProductInfo GetProductInfo(string productId)
        {
            if (_productList != null)
            {
                foreach (var productInfo in _productList.ProductInfos)
                {
                    if (productInfo.id.Equals(productId))
                    {
                        return productInfo;
                    }
                }
            }

            return null;
        }
        
        public static void Pay(string productId)
        {
            Burnicle.CrossPlatform.Pay(productId);
        }

        public static void OnPaySuccess(string productId)
        {
        }
    }
}