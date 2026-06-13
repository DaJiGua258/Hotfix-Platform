using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ABToolPackage
{
    public class ABNetInfo : MonoBehaviour
    {
        public static string compareFileName = "/ABCompareFile.txt";
        public static string compareFileNameTMP = "/ABCompareFile_TMP.txt";
        public static string serverIP = "ftp://127.0.0.1";
        public static string userName = "user";
        public static string passWord = "123456";
        public static string aBFileDir = "";

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
