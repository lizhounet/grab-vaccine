using grab_vaccine.conf;
using grab_vaccine.exception;
using Masuit.Tools.Security;
using NewLife.Log;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.service
{
    public class HttpService
    {
        private RestClient client = new RestClient("https://miaomiao.scmttec.com");

        public string SecKill()
        {
            var request = new RestRequest("resource/{id}", Method.POST);
            IRestResponse response = client.Execute(request);
            return null;
        }
        /// <summary>
        /// log接口，不知道有何作用，但返回值会设置一个名为tgw_l7_route的cookie
        /// </summary>
        /// <param name="vaccineId">vaccineId 疫苗ID</param>
        public void Log(String vaccineId)
        {
            String path = "seckill/seckill/log.do";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("id", vaccineId);
            Send(path, param, null);
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
            Dictionary<string, string> commonHeaders = GetCommonHeader();
            request.AddHeaders(commonHeaders);
            if (extHeader != null && extHeader.Count > 0)
            {
                request.AddHeaders(extHeader);
            }
            XTrace.WriteLine($"发送请求：{client.BaseUrl}{path}");
            IRestResponse response = client.Execute(request);
            XTrace.WriteLine($"返回值：{response.Content}");
            if (response.IsSuccessful)
            {
                JObject json = JObject.Parse(response.Content);
                if ("0000".Equals(json["code"]))
                {
                    return json["data"].ToString();
                }
                else
                {
                    throw new BusinessException(json["msg"].ToString());
                }
            }
            else
            {
                throw new BusinessException("请求异常");
            }
        }
        /// <summary>
        /// 解析返回值需要设置的cookie
        /// </summary>
        /// <param name="response"></param>
        private void dealHeader(IRestResponse response)
        {
            IList<RestResponseCookie> cookies = response.Cookies;
            if (cookies.Count > 0)
            {
                foreach (var item in cookies)
                {
                    string cookie = item.Value.Split(";")[0].Split(":")[1].Trim();
                    string[] split = cookie.Split("=");
                    VaccineConfig.Instance.Cookies.Add(split[0], cookie);
                }
            }
        }
        private Dictionary<string, string> GetCommonHeader()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.1.1; SM-N960F Build/JLS36C; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/74.0.3729.136 Mobile Safari/537.36 MMWEBID/1042 MicroMessenger/7.0.15.1680(0x27000F34) Process/appbrand0 WeChat/arm32 NetType/WIFI Language/zh_CN ABI/arm32");
            headers.Add("Referer", "https://servicewechat.com/wxff8cad2e9bf18719/2/page-frame.html");
            headers.Add("tk", VaccineConfig.Instance.Tk);
            headers.Add("Accept", "application/json, text/plain, */*");
            headers.Add("Host", "miaomiao.scmttec.com");
            if (VaccineConfig.Instance.Cookies.Count > 0)
            {
                Dictionary<string, string>.ValueCollection values = VaccineConfig.Instance.Cookies.Values;
                String cookie = String.Join("; ", VaccineConfig.Instance.Cookies.Values);
                XTrace.WriteLine($"cookie is {cookie}");
                headers.Add("Cookie", cookie);
            }
            return headers;
        }

        private String EccHs(String seckillId, String st)
        {
            String salt = "ux$ad70*b";
            int memberId = VaccineConfig.Instance.MemberId;
            String md5 = (seckillId + memberId + st).MDString();
            return (md5 + salt).MDString();
        }
    }
}
