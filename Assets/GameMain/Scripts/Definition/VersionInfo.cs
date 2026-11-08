
namespace MergeSword
{
    public class VersionInfo
    {
        // 资源版本号（来自构建生成的 ResourceVersion.json 中 ResourceVersion 字段）
        // 与 InternalResourceVersion 含义相同，主要用于兼容 JSON 反序列化
        public int ResourceVersion
        {
            get;
            set;
        }

        // 资源更新下载地址
        public string UpdatePrefixUri
        {
            get;
            set;
        }

        // 资源版本列表长度
        public int VersionListLength
        {
            get;
            set;
        }

        // 资源版本列表哈希值
        public int VersionListHashCode
        {
            get;
            set;
        }

        // 资源版本列表压缩后长度
        public int VersionListCompressedLength
        {
            get;
            set;
        }

        // 资源版本列表压缩后哈希值
        public int VersionListCompressedHashCode
        {
            get;
            set;
        }
    }
}
