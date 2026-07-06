using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ABToolPackage
{
    public class ABUploadTool : EditorWindow
    {
        [MenuItem("Tool/ABTool")]
        private static void ShowWindow()
        {
            ABUploadTool window = (ABUploadTool)GetWindowWithRect(typeof(ABUploadTool), new Rect(0, 0, 400, 400), false, "ABToola");
            window.Show();
        }

        private void OnGUI()
        {
            var areaRect = new Rect(10, 10, 350, 350);
            GUILayout.BeginArea(areaRect);

            EditorGUIUtility.labelWidth = areaRect.width * 0.25f; // label 占 25%，textbox 占 75%
            ABNetInfo.serverIP = EditorGUILayout.TextField("FTP地址:", ABNetInfo.serverIP);
            ABNetInfo.userName = EditorGUILayout.TextField("用户名:", ABNetInfo.userName);
            ABNetInfo.passWord = EditorGUILayout.TextField("密码:", ABNetInfo.passWord);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("本地AB包绝对路径:");
            ABNetInfo.aBFileDir = EditorGUILayout.TextField(ABNetInfo.aBFileDir);

            GUILayout.Space(20);

            if (GUILayout.Button("Lua转TXT保存到LuaTxt", GUILayout.Height(25)))
            {
                LuaToTxt();
            }

            GUILayout.Space(5);

            // 居中且拉伸的按钮
            if (GUILayout.Button("生成本地AB包的对比文件", GUILayout.Height(25)))
            {
                CreateABCompareFile(ABNetInfo.aBFileDir);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("保存资源到StreamingAssets文件夹", GUILayout.Height(25)))
            {
                MoveABToSteamingAssets();
            }

            GUILayout.Space(5);


            // 全宽拉伸按钮
            if (GUILayout.Button("上传StreamingAssets中的AB包和对比文件", GUILayout.Height(25)))
            {
                UploadAllABFile();
            }

            GUILayout.Space(5);

            // 全宽拉伸按钮
            if (GUILayout.Button("测试", GUILayout.Height(25)))
            {
                // Debug.Log("Assets/Scripts/Test1.cs");
                // Debug.Log(GetMD5("Assets/Scripts/Test1.cs"));
                // MoveABToSteamingAssets();

                // CreateABCompareFile(Application.streamingAssetsPath);

                // Debug.Log(Application.streamingAssetsPath);
            }

            GUILayout.EndArea();
        }

        #region Button方法

        /// <summary>
        /// 将当前选中的文件中的AB包移动到制定目录
        /// </summary>
        private void MoveABToSteamingAssets()
        {
            // 通过编辑器Selection类中的方法 获取再Project窗口中选中的资源
            UnityEngine.Object[] selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if(selectedAssets.Length == 0)
            {
                return;
            }

            // 遍历选中的资源对象
            foreach(UnityEngine.Object asset in selectedAssets)
            {
                // 获取 当前 完整路径，以及文件名称
                string filePath = AssetDatabase.GetAssetPath(asset);
                string fileName = filePath.Substring(filePath.LastIndexOf('/'));

                // 对于有后缀的（非AB包文件）跳过
                if(fileName.IndexOf(".") != -1) continue;

                // 获取 目标 完整路径
                string newPath = "Assets/StreamingAssets" + fileName;

                // 复制文件
                AssetDatabase.CopyAsset(filePath, newPath);
            }

            CreateABCompareFile(Application.streamingAssetsPath);

            AssetDatabase.Refresh();
        }


        /// <summary>
        ///
        /// </summary>
        private void UploadAllABFile()
        {
            // 获取文件夹信息
            DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
            FileInfo[] fileInfos = dir.GetFiles();

            // 遍历制定类型文件并上传
            foreach (var info in fileInfos)
            {
                if(info.Extension == "" || info.Extension == ".txt")
                {
                    FtpUploadFile(info.FullName, info.Name);
                }
            }
        }

        /// <summary>
        /// 自动从 Assets/Lua/ 下获取所有 .lua 文件，转为 .txt 保存到 LuaTxt 并设置 AB 标记为 lua
        /// </summary>
        private void LuaToTxt()
        {
            string targetDir = "Assets/LuaTxt";

            // 确保目标目录存在
            if (!AssetDatabase.IsValidFolder(targetDir))
                AssetDatabase.CreateFolder("Assets", "LuaTxt");

            // 1. 自动递归获取 Assets/Lua/ 下所有 .lua 文件
            string dataPath = Application.dataPath;
            string[] allLuaFiles = Directory.GetFiles(dataPath + "/Lua", "*.lua", SearchOption.AllDirectories);
            if (allLuaFiles.Length == 0)
            {
                Debug.LogWarning("Assets/Lua/ 下未找到 .lua 文件");
                return;
            }

            // 2. 全部转为 Unity 相对路径并复制到 LuaTxt
            List<string> convertedPaths = new List<string>();
            foreach (string fullPath in allLuaFiles)
            {
                string unityPath = "Assets" + fullPath.Substring(dataPath.Length).Replace('\\', '/');
                string fileName = Path.GetFileNameWithoutExtension(unityPath);
                string targetPath = targetDir + "/" + fileName + ".lua.txt";

                // 删除旧文件，避免 CopyAsset 失败
                if (File.Exists(targetPath))
                    AssetDatabase.DeleteAsset(targetPath);

                if (AssetDatabase.CopyAsset(unityPath, targetPath))
                {
                    convertedPaths.Add(targetPath);
                    Debug.Log("已复制：" + unityPath + " → " + targetPath);
                }
            }

            // 3. 设置 AB 标签为 lua（对新文件生效，旧文件保持）
            foreach (string path in convertedPaths)
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = "lua";
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Lua 转换完成：共 {convertedPaths.Count} 个文件，已标记 AB = lua");
        }

        #endregion

        #region 工具类


        /// <summary>
        /// 依据传入的文件夹路径生成当前文件夹下的对比文件
        /// </summary>
        /// <param name="dirPath"></param>
        private void CreateABCompareFile(string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            FileInfo[] fileInfos = dir.GetFiles();

            string compareInfo = "";

            foreach (var info in fileInfos)
            {
                if(info.Extension != "") continue;

                compareInfo += info.Name + "_" + info.Length + "_" + ABNetInfo.GetMD5(info.FullName);
                compareInfo += '\n';
            }

            // 去掉信息尾部的 \n
            compareInfo = compareInfo.Substring(0, compareInfo.Length - 1);

            // 存储拼接好的 AB 包资源信息
            File.WriteAllText(dirPath + ABNetInfo.compareFileName, compareInfo);
            AssetDatabase.Refresh();

            Debug.Log("已生成的对比信息： \n" + compareInfo);
        }

        /// <summary>
        /// 将文件上传到FTP服务器中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async void FtpUploadFile(string filePath, string fileName)
        {
            // 主线程读取配置，避免后台线程访问 EditorPrefs
            string serverIP = ABNetInfo.serverIP;
            string userName = ABNetInfo.userName;
            string passWord = ABNetInfo.passWord;

            await Task.Run(() =>
            {
                try
                {
                    // 1. 创建FTP连接
                    FtpWebRequest req = FtpWebRequest.Create(serverIP + "/" + fileName) as FtpWebRequest;

                    // 2. 设置通信凭证
                    NetworkCredential n = new NetworkCredential(userName, passWord);
                    req.Credentials = n;

                    // 3. 设置链接配置
                    req.Proxy = null;  // 设置代理为null
                    req.KeepAlive = false;  // 请求完毕后 是否关闭控制连接
                    req.Method = WebRequestMethods.Ftp.UploadFile;  // 操作命令-上传
                    req.UseBinary = true;  // 指定传输的类型 2进制
                    req.UsePassive = true;  // 使用被动模式

                    // 4. 上传文件
                    Stream upLoadStream = req.GetRequestStream();

                    // 读取文件，写入流对象
                    using(FileStream file = File.OpenRead(filePath))
                    {
                        byte[] buffer = new byte[2048];  // 定义一个缓冲区
                        int contentLength = file.Read(buffer, 0, buffer.Length);  // 第一次从文件读取到缓冲区，并返回实际的读取长度

                        while(contentLength != 0)
                        {
                            // 从 buffer 中提取之前获取到的 contentLength 长度的所有字节，写入到上传流中
                            upLoadStream.Write(buffer, 0, contentLength);
                            contentLength = file.Read(buffer, 0, buffer.Length);  // 在此读取到 buffer，并返回实际读取到的长度
                        }

                        file.Close();
                        upLoadStream.Close();
                    }

                    Debug.Log("上传成功：" + fileName);
                }
                catch(Exception ex)
                {
                    Debug.Log("上传失败：" + fileName + '\n' + ex);
                }
            });
        }

        #endregion
    }
}
