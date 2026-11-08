using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MergeSword
{
    /// <summary>
    /// 解析 Persistent 存档文件（GFS 二进制格式），提取永久特征码。
    /// 通过手动解析 byte[] 演示 GFS 容器的二进制布局。
    /// </summary>
    public static class SaveFileInspector
    {
        private const string FileName = "GameFrameworkSetting.dat";

        [MenuItem("Tools/存档解析 - 特征码")]
        public static void InspectSaveFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, FileName);

            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("存档解析", $"未找到存档文件：\n{filePath}", "确定");
                return;
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            ParseSaveFile(bytes);
        }

        private static void ParseSaveFile(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
            {
                Debug.LogError("存档文件无效：长度不足");
                return;
            }

            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                // ---- Header ----
                byte h0 = reader.ReadByte();
                byte h1 = reader.ReadByte();
                byte h2 = reader.ReadByte();
                byte version = reader.ReadByte();

                string magic = $"{(char)h0}{(char)h1}{(char)h2}";
                if (magic != "GFS")
                {
                    Debug.LogError($"存档头无效：期望 'GFS'，实际 '{magic}'");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"=== GFS 存档解析 ===");
                sb.AppendLine($"魔数: {magic}");
                sb.AppendLine($"版本: {version}");
                sb.AppendLine($"文件大小: {bytes.Length} 字节");
                sb.AppendLine();

                // ---- 键值对数量（7-bit 编码）----
                int entryCount = Read7BitEncodedInt32(reader);
                sb.AppendLine($"条目总数: {entryCount}");
                sb.AppendLine(new string('-', 40));

                string identity = null;

                for (int i = 0; i < entryCount; i++)
                {
                    string key = reader.ReadString();
                    string value = reader.ReadString();

                    if (key == "__StorageIdentity__")
                    {
                        identity = value;
                        sb.AppendLine($"[永久特征码] {value}");
                    }
                    else
                    {
                        sb.AppendLine($"[存档数据] Key: {key}, Value 长度: {value?.Length ?? 0} 字符");
                    }
                }

                sb.AppendLine(new string('-', 40));
                if (identity == null)
                    sb.AppendLine("⚠ 未找到永久特征码（__StorageIdentity__）");

                sb.AppendLine(new string('=', 40));
                Debug.Log(sb.ToString());

                EditorUtility.DisplayDialog("存档解析完成", $"解析日志已输出到 Console 窗口\n路径：{Path.Combine(Application.persistentDataPath, FileName)}", "确定");
            }
        }

        /// <summary>
        /// 从 GFS 存档 byte[] 中解析出永久特征码，未找到时返回 null。
        /// 只遍历到找到目标键即返回，不解析全部数据。
        /// </summary>
        public static string ParseIdentity(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;

            try
            {
                using (var stream = new MemoryStream(bytes, false))
                using (var reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    // 跳过 4 字节头 (GFS + version)
                    reader.ReadByte(); reader.ReadByte(); reader.ReadByte(); reader.ReadByte();

                    // 条目数（7-bit 编码）
                    int count = Read7BitEncodedInt32(reader);
                    for (int i = 0; i < count; i++)
                    {
                        string key = reader.ReadString();
                        string value = reader.ReadString();
                        if (key == "__StorageIdentity__")
                            return value;
                    }
                }
            }
            catch
            {
                // 格式异常返回 null
            }

            return null;
        }

        /// <summary>
        /// 手动解析 7-bit 编码的 Int32（与 DataStorage.Read7BitEncodedInt32 一致）。
        /// </summary>
        public static int Read7BitEncodedInt32(BinaryReader reader)
        {
            int value = 0;
            int shift = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                value |= (b & 0x7F) << shift;
                shift += 7;
            }
            while ((b & 0x80) != 0);
            return value;
        }
    }
}
