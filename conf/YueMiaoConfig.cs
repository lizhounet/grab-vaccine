using grab_vaccine.model;
using grab_vaccine.utils;
using Microsoft.Extensions.Configuration;
using NewLife.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace grab_vaccine.conf
{
    /// <summary>
    /// 约苗配置信息
    /// </summary>
    public class YueMiaoConfig
    {
        /// <summary>
        /// 接种成员ID
        /// </summary>
        public int MemberId { set; get; }
        /// <summary>
        /// 接种成员身份证号码
        /// </summary>
        public string IdCard { set; get; }
        /**
         * 接种成员姓名
         */
        public string MemberName { set; get; }
        public string Tk { set; get; }
        /// <summary>
        /// 要抢的疫苗信息
        /// </summary>
        public VaccineInfo Vaccine { set; get; }

        /**
         * 调用接口时返回的set-cookie
         */
        public ConcurrentDictionary<string, string> Cookies { set; get; } = new ConcurrentDictionary<string, string>();

        private static YueMiaoConfig instance = null;
        public static YueMiaoConfig Instance
        {
            set { instance = value; }
            get
            {
                return instance;
            }
        }

        static YueMiaoConfig()
        {
            ReloadYueMiaoConfig();
        }
        public static void ReloadYueMiaoConfig()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("//appsettings.json", true, true);
            var config = build.Build();
            instance = config.GetSection("YueMiaoConfig").Get<YueMiaoConfig>();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "reqHeader.txt");
            //   path = @"D:\zhouli\work\code\java\temp\reqHeader.txt";
            string reqHeader = File.ReadAllText(path);
            //设置cookie和tk
            string[] data = ParseUtil.ParseHeader(reqHeader);
            instance.Tk = data[0];
            string[] vs = data[1].Replace(" ", "").Split(";");
            foreach (var s in vs)
            {
                string[] vs1 = s.Split("=");
                instance.Cookies.TryAdd(vs1[0], vs1[1]);
            }
            XTrace.WriteLine("YueMiaoConfig初始化");
        }

    }
}
