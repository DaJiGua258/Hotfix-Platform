using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace ABToolPackage
{
    public class ABDownload : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        private static ABDownload instance;

        public static ABDownload Instance
        {
            get
            {
                if(instance == null)
                {
                    GameObject obj = new GameObject("ABDownload");
                    instance = obj.AddComponent<ABDownload>();
                }

                return instance;
            }
        }

        /// <summary>
        /// AB 包信息
        /// </summary>
        public class ABInfo
        {
            public string Name;
            public long Size;
            public string MD5;

            public ABInfo(string name, string size, string md5)
            {
                Name = name;
                Size = long.Parse(size);
                MD5 = md5;
            }
        }

        // 远端 AB包 信息字典，用于与本地进行对比
        // 本地 AB包 信息字典，用于与远端进行对比
        private Dictionary<string, ABInfo> remoteABInfoDic = new Dictionary<string, ABInfo>();
        private Dictionary<string, ABInfo> localABInfoDic = new Dictionary<string, ABInfo>();

        // 对比后得到的待下载 AB包 列表
        private List<string> downloadList = new List<string>();

    
        #region 下载流程
        private List<Func<Task>> _taskList = new List<Func<Task>>();
        private void AddTask(Func<Task> task)
        {
            _taskList.Add(task);
        }
        /// <summary>
        /// 添加一个任务到任务列表中
        /// </summary>
        /// <param name="action">要执行的操作，类型为Action</param>
        private void AddTask(Action action)
        {
            // 将传入的action包装成一个Task并添加到任务列表中
            // 包装后的action在执行后会返回一个已完成的Task
            _taskList.Add(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public async void CheckUpdate(Action onCompleted = null)
        {
            string remoteInfo = "";

            remoteABInfoDic.Clear();
            localABInfoDic.Clear();
            downloadList.Clear();

            AddTask(async () => // 异步
            {
                print("1. 开始：下载远端对比文件...");
                await DownRemoteCompareFile();
                print("2. 完成：远端对比文件下载...");
            });  

            AddTask(() =>  // 同步
            {
                print("3. 开始：读取对比文件信息...");
                // 读取下载下来的对比信息文件
                remoteInfo = File.ReadAllText(Application.persistentDataPath + ABNetInfo.compareFileNameTMP);
                GetCompareFileInfo(remoteInfo, remoteABInfoDic);  // 拆分从文件中读出来的信息
                print("4. 完成：读取对比文件信息...");
            });

            AddTask(() =>  // 同步
            {
                print("5. 开始：检查本地对比文件位置...");
                GetCheckLocalCompareFile(); 
            });

            AddTask(() =>  // 同步
            {
                print("6. 开始：对比本地与远端的文件信息...");
                CompareLocalAndRemote();
                
            });

            AddTask(async () =>  // 异步
            {
                print("7. 开始：下载AB包...");
                print("下载路径：" + Application.persistentDataPath);
                await DownloadABFiles();
            });

            AddTask(() =>
            {
                print("8. 开始：更新本地的对比文件信息...");
                // 将更新后的AB包对比文件信息（也就是之前从远端读取的）
                // 更新到 persistentDataPath 文件下
                File.WriteAllText(Application.persistentDataPath + ABNetInfo.compareFileName, remoteInfo);
            });
            
            

            // 执行异步任务列表
            foreach(var task in _taskList)
            {
                await task();
            }

            onCompleted?.Invoke();
        }

        #endregion


        #region 工具类
        
        /// <summary>
        /// 依据目录和文件名，下载单个文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public bool DownLoadFile(string fileName, string localPath, string serverIP, string userName, string passWord)
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
                req.Method = WebRequestMethods.Ftp.DownloadFile;  // 操作命令-下载
                req.UseBinary = true;  // 指定传输的类型 2进制
                req.UsePassive = true;  // 使用被动模式

                // 4. 下载文件
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                Stream downloadStream = res.GetResponseStream();

                // 创建文件，将数据写入本地文件
                using(FileStream file = File.Create(localPath))
                {
                    byte[] buffer = new byte[2048];  // 定义一个缓冲区
                    int contentLength = downloadStream.Read(buffer, 0, buffer.Length);  // 第一次从下载的流读取到缓冲区，并返回实际的读取长度

                    while(contentLength != 0)
                    {
                        // 通过 buffer 将读取的字节，写入到文件中
                        file.Write(buffer, 0, contentLength);
                        contentLength = downloadStream.Read(buffer, 0, buffer.Length);  // 再次从下载的流中，读取字节到 buffer 中
                    }

                    file.Close();
                    downloadStream.Close();

                    print("下载成功：" + fileName);

                    return true;
                }
            }
            catch(Exception ex)
            {
                print("下载失败：" + fileName + '\n' + ex);
                
                return false;
            }
        }

        /// <summary>
        /// 下载远端对比文件
        /// </summary>
        private async Task DownRemoteCompareFile()
        {
            // 只能从主线程调用，所以放到外面
            string path = Application.persistentDataPath;
            string ip = ABNetInfo.serverIP;
            string user = ABNetInfo.userName;
            string pwd = ABNetInfo.passWord;
            await Task.Run(() =>
            {
                DownLoadFile(ABNetInfo.compareFileName, path + ABNetInfo.compareFileNameTMP, ip, user, pwd);
            });
        }

        /// <summary>
        /// 从对比文件中解析AB包信息，存入字典
        /// </summary>
        private void GetCompareFileInfo(string fileInfo, Dictionary<string, ABInfo> dic)
        {
            string[] infos = fileInfo.Split('\n');
            foreach (var info in infos)
            {
                if(info == "") continue;
                // 拆分远端的对比信息，并保存
                var i = info.Split('_');

                // print(info);
                
                dic.Add(i[0], new ABInfo(i[0], i[1], i[2]));
            }
        }

        /// <summary>
        /// 检查本地的对比文件的存在位置，判断是否有默认资源
        /// </summary>
        private void GetCheckLocalCompareFile()
        {
            // TODO: 使用 UnityWebRequest 去加载本地文件（解析移动平台压缩包）
            string pathP = Application.persistentDataPath + ABNetInfo.compareFileName;
            string pathS = Application.streamingAssetsPath + ABNetInfo.compareFileName;

            // 检查可读可写文件夹内是否存在 对比文件
            if(File.Exists(pathP))
            {
                print("persistentDataPath 存在对比文件，在此处生成对比文件");
                GetCompareFileInfo(File.ReadAllText(pathP), localABInfoDic);
            }
            // 检查可读文件夹内是否存在 对比文件
            else if(File.Exists(pathS))
            {
                print("streamingAssetsPath 存在对比文件，在此处生成对比文件");
                GetCompareFileInfo(File.ReadAllText(pathS), localABInfoDic);
            }
            // foreach (var item in localABInfoDic)
            // {
            //     print(item.Key + "  " + item.Value.Name + "  " + item.Value.MD5 + "  ");
            // }
        }

        /// <summary>
        /// 对比本地与远端的AB包信息，完成两个字典与下载列表的构建
        /// </summary>
        private void CompareLocalAndRemote()
        {
            // 开始遍历远端 AB包 字典信息
            foreach (var abName in remoteABInfoDic.Keys)
            {
                // 发现本地没有的AB包，加入下载列表
                if(!localABInfoDic.ContainsKey(abName))
                {
                    downloadList.Add(abName);
                }
                // 发现本地存在的AB包，则判断MD5码
                else
                {
                    // MD5码不同，则加入下载列表
                    if(localABInfoDic[abName].MD5 != remoteABInfoDic[abName].MD5)
                    {
                        downloadList.Add(abName);
                    }
                }

                // 每次检查完一个AB包名，则移除
                // 剩下的就是远端也不存在的AB包，即冗余的资源
                // 后续删除
                localABInfoDic.Remove(abName);
            }

            foreach(var abName in downloadList)
            {
                print(abName);
            }

            // 删除本地冗余的AB包
            foreach (var abName in localABInfoDic.Keys)
            {
                string path = Application.persistentDataPath + "/" + abName;
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// 下载 下载列表中所有 AB包 文件
        /// </summary>
        /// <returns></returns>
        private async Task DownloadABFiles()
        {
            // 只能从主线程调用，所以放到外面
            string ip = ABNetInfo.serverIP;
            string user = ABNetInfo.userName;
            string pwd = ABNetInfo.passWord;

            // 遍历下载列表进行下载
            foreach(var abName in downloadList)
            {
                print("开始下载：" + abName);
                string path = Application.persistentDataPath + "/" + abName;
                // 异步下载
                await Task.Run(() =>
                {
                    DownLoadFile(abName, path, ip, user, pwd);
                });
            }
        }
        
        #endregion
    }
}
