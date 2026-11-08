using GameFramework.DataTable;
using System;
using GameFramework;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    public static class DataTableExtension
    {
        private const string DataRowClassPrefixName = "BurnicleFramework.Generated.DR";
        public static void LoadDataTable(this DataTableComponent dataTableComponent, string dataTableName, string dataTableAssetName, object userData)
        {
            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            string[] splitedNames = dataTableName.Split('_');
            if (splitedNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            var dataRowTypeName = DataRowClassPrefixName + splitedNames[0];
            Type dataRowType = Utility.Assembly.GetType(dataRowTypeName);
            if (dataRowType == null)
            {
                Log.Warning("Can not get data row type with class name '{0}'.", dataRowTypeName);
                return;
            }

            string name = splitedNames.Length > 1 ? splitedNames[1] : null;
            DataTableBase dataTable = dataTableComponent.CreateDataTable(dataRowType, name);
            dataTable.ReadData(dataTableAssetName, Constant.AssetPriority.DataTableAsset, userData);
        }

        public static T GetDataTableRow<T>(this DataTableComponent dataTableComponent, int id) where T : DataRowBase
        {
            var dataTable = dataTableComponent.GetDataTable<T>();
            if (dataTable == null)
            {
                // 数据表尚未加载（如预加载完成前的点击先于该表创建）时按“未找到行”处理，由调用方统一判空
                return null;
            }
            return dataTable.GetDataRow(id);
        }
    }
}
