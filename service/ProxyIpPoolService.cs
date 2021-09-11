using grab_vaccine.exception;
using Microsoft.Extensions.Configuration;
using NewLife.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.service
{
    public class ProxyIpPoolService
    {
        private RestClient client = null;
        public ProxyIpPoolService()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("//appsettings.json", true, true);
            var config = build.Build();
            string baseUrl = config.GetSection("ProxyIpPoolAdress").Get<string>();
            client = new RestClient(baseUrl);
        }

        /// <summary>
        /// 随机获取一个代理
        /// </summary>
        /// <param name="type">type=https 过滤支持https的代理</param>
        /// <returns></returns>
        public ProxyIpInfo Get(string type = null)
        {
            try
            {
                Dictionary<String, String> param = new Dictionary<string, string>(1);
                if (!string.IsNullOrEmpty(type))
                {
                    param.Add("type", type);
                }
                string content = Send("get", param, null);
                return JsonConvert.DeserializeObject<ProxyIpInfo>(content);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            return null;
        }
        /// <summary>
        /// 获取所有代理
        /// </summary>
        /// <param name="type">type=https 过滤支持https的代理</param>
        /// <returns></returns>
        public List<ProxyIpInfo> All(string type = null)
        {
            try
            {
                Dictionary<String, String> param = new Dictionary<string, string>(1);
                if (!string.IsNullOrEmpty(type))
                {
                    param.Add("type", type);
                }
                string content = Send("all", param, null);
                return JsonConvert.DeserializeObject<List<ProxyIpInfo>>(content);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            return new List<ProxyIpInfo>();
        }


        /// <summary>
        /// 查看代理数量
        /// </summary>
        /// <returns></returns>
        public ProxyIpCount Count()
        {
            try
            {
                string content = Send("count", null, null);
                JObject json = JObject.Parse(content);
                return JsonConvert.DeserializeObject<ProxyIpCount>(json["count"].ToString());
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            return null;
        }
        /// <summary>
        /// 获取所有代理
        /// </summary>
        /// <param name="type">type=https 过滤支持https的代理</param>
        /// <returns></returns>
        public List<string> AllTest()
        {
            string url = "http://www.zdopen.com/PrivateProxy/GetIP/?api=202109112234541589&akey=353e4c90bccf6a67&count=20&fitter=2&timespan=6&type=3";
            RestClient client = new RestClient(url);
            var request = new RestRequest("", Method.GET);
            IRestResponse restResponse = client.Execute(request);

            JObject json = JObject.Parse(restResponse.Content);
            return json["data"]["proxy_list"].Select(t => $"{t["ip"]}:{t["port"]}").ToList();

        }
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="param">请求参数</param>
        /// <param name="extHeader">Header</param>
        /// <returns></returns>
        private string Send(String path, Dictionary<String, String> param, Dictionary<string, string> extHeader)
        {
            var request = new RestRequest(path, Method.GET);
            if (param != null && param.Count > 0)
            {
                foreach (var item in param)
                {
                    request.AddParameter(item.Key, item.Value);
                }
            }
            XTrace.WriteLine($"发送请求：{client.BaseUrl}{path}");
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return response.Content;
            }
            else
            {
                throw new BusinessException($"请求异常：{response.StatusDescription}");
            }
        }
    }
    /// <summary>
    /// 代理IP信息
    /// </summary>
    public class ProxyIpInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string anonymous { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int check_count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int fail_count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string https { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string last_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string last_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string proxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string region { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string source { get; set; }
    }
    public class ProxyIpCount
    {
        /// <summary>
        /// 
        /// </summary>
        public int https { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int total { get; set; }
    }
}
