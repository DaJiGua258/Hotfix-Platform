using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ABToolPackage
{
    public class ABNetInfo : MonoBehaviour
    {
        public static string compareFileName = "/ABCompareFile.txt";
        public static string compareFileNameTMP = "/ABCompareFile_TMP.txt";

        private const string PrefsKey_IP = "ABTool_serverIP";
        private const string PrefsKey_User = "ABTool_userName";
        private const string PrefsKey_Pwd = "ABTool_passWord";
        private const string PrefsKey_Dir = "ABTool_aBFileDir";

        public static string serverIP
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetString(PrefsKey_IP, "ftp://127.0.0.1");
#else
                return "ftp://127.0.0.1";
#endif
            }
            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetString(PrefsKey_IP, value);
#endif
            }
        }

        public static string userName
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetString(PrefsKey_User, "user");
#else
                return "user";
#endif
            }
            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetString(PrefsKey_User, value);
#endif
            }
        }

        public static string passWord
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetString(PrefsKey_Pwd, "123456");
#else
                return "123456";
#endif
            }
            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetString(PrefsKey_Pwd, value);
#endif
            }
        }

        public static string aBFileDir
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetString(PrefsKey_Dir, "");
#else
                return "";
#endif
            }
            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetString(PrefsKey_Dir, value);
#endif
            }
        }

        /// <summary>
        /// 依据路径的目标文件，返回一串MD5码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5(string filePath)
        {
            using(FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                // 生成MD5码
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5Info = md5.ComputeHash(fileStream);  // 获取16个字节数组
                
                fileStream.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < md5Info.Length; i++)
                {
                    // 将16个字节数组转为16进制，再拼接字符串
                    sb.Append(md5Info[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
