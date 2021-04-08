using FrpGUI.Util;
using System;
using System.Text.RegularExpressions;

namespace WakeOnLan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("网络唤醒");
            Console.WriteLine();
            Console.WriteLine("0:打开HTTP服务器");
            Console.WriteLine("1:输入MAC进行唤醒");
            var hosts = Config.GetHosts();
            int i = 2;
            foreach (var host in hosts)
            {
                Console.WriteLine(i++ + "：唤醒" + host.Name);
            }
            Console.WriteLine();
            string input = "";
            int index = -1;
            while (!(int.TryParse(input, out index) && index >= 0 && index < i))
            {
                Console.Write("请输入序号：");
                input = Console.ReadLine().Trim();
            }
            Console.WriteLine();
            switch (index)
            {
                case 0:
                    new HttpServerHelper().Start();
                    Console.WriteLine("HTTP服务器已启动");
                    while (true)
                    {
                        Console.ReadKey();
                    }
                case 1:
                    Console.Write("请输入MAC，分隔符可以为\":\"、\"-\"或空：");
                    string mac = Console.ReadLine().Trim();
                    while (!Regex.IsMatch(mac, "^([0-9A-Fa-f]{2}[:-]?){5}([0-9A-Fa-f]{2})$"))
                    {
                        Console.WriteLine("MAC地址格式错误");
                        Console.Write("请输入MAC，分隔符可以为\":\"、\"-\"或空：");
                        mac = Console.ReadLine().Trim();
                    }
                    WOL.WakeOnLan(mac).Wait();
                    Console.WriteLine("按任意键退出");
                    Console.ReadKey();
                    break;

                default:
                    string mac2 = hosts[index - 2].Mac;
                    WOL.WakeOnLan(mac2).Wait();
                    Console.WriteLine("按任意键退出");
                    Console.ReadKey();
                    break;
            }
        }
    }
}