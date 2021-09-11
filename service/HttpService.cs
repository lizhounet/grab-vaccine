using grab_vaccine.conf;
using grab_vaccine.exception;
using grab_vaccine.model;
using Masuit.Tools.Security;
using NewLife.Log;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace grab_vaccine.service
{
    public class HttpService
    {

        /// <summary>
        /// 获取秒杀资格
        /// </summary>
        /// <param name="seckillId">疫苗ID</param>
        /// <param name="vaccineIndex">固定1</param>
        /// <param name="linkmanId">接种人ID</param>
        /// <param name="idCard">接种人身份证号码</param>
        /// <param name="st">时间戳</param>
        /// <param name="webProxy">代理ip</param>
        /// <returns>返回订单ID</returns>
        public String secKill(String seckillId, String vaccineIndex, String linkmanId, String idCard, String st, WebProxy webProxy = null)
        {
            String path = "seckill/seckill/subscribe.do";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("seckillId", seckillId);
            param.Add("vaccineIndex", vaccineIndex);
            param.Add("linkmanId", linkmanId);
            param.Add("idCardNo", idCard);
            param.Add("random", new Random().Next(0,1000).ToString());
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("ecc-hs", EccHs(seckillId, st));
            return Send(path, param, header, webProxy);
        }

        /// <summary>
        /// 获取加密参数st
        /// </summary>
        /// <param name="vaccineId">疫苗ID</param>
        /// <returns></returns>
        public String GetSt(String vaccineId)
        {
            String path = "seckill/seckill/checkstock2.do";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("id", vaccineId);
            String json = Send(path, param, null);
            JObject jsonObject = JObject.Parse(json);
            return jsonObject["st"].ToString();
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
        /// 获取接种人信息
        /// </summary>
        /// <returns></returns>
        public List<Member> GetMembers()
        {
            String path = "seckill/linkman/findByUserId.do";
            String json = Send(path, null, null);
            return JsonConvert.DeserializeObject<List<Member>>(json);

        }
        /// <summary>
        /// 获取疫苗列表
        /// </summary>
        /// <param name="regionCode">行政区域code</param>
        /// <returns></returns>
        public List<VaccineInfo> GetVaccineList(string regionCode, WebProxy webProxy = null)
        {
            List<VaccineInfo> vaccineInfos = null;
            //接口出错最多重试10次
            int retryFrequency = 1;
            while (retryFrequency < 10)
            {
                try
                {
                    String path = "seckill/seckill/list.do";
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("offset", "0");
                    param.Add("limit", "100");
                    param.Add("regionCode", regionCode);
                    XTrace.WriteLine($"{regionCode}查询疫苗");
                    String json = Send(path, param, null, webProxy);
                    vaccineInfos = JsonConvert.DeserializeObject<List<VaccineInfo>>(json);
                    break;
                }
                catch (BusinessException bex)
                {
                    XTrace.WriteLine(bex.Message);
                    retryFrequency++;
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                    retryFrequency++;
                }
            }
            return vaccineInfos;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="param">请求参数</param>
        /// <param name="extHeader">Header</param>
        /// <param name="webProxy">代理ip</param>
        /// <returns></returns>
        private string Send(String path, Dictionary<String, String> param, Dictionary<string, string> extHeader, WebProxy webProxy = null)
        {
            RestClient client = new RestClient("https://miaomiao.scmttec.com");
            if (webProxy != null)
            {
                XTrace.WriteLine($"设置代理ip：{webProxy.Address}");
                client.Proxy = webProxy;
            }
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
            dealHeader(response);
            // XTrace.WriteLine($"返回值：{response.Content}");
            if (response.IsSuccessful)
            {
                JObject json = JObject.Parse(response.Content);
                if ("0000".Equals(json["code"].ToString()))
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
                throw new BusinessException($"请求异常：{response.StatusDescription}");
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
                    if (YueMiaoConfig.Instance.Cookies.ContainsKey(item.Name))
                    {
                        YueMiaoConfig.Instance.Cookies.Remove(item.Name);
                    }
                    YueMiaoConfig.Instance.Cookies.TryAdd(item.Name, item.Value);
                }
            }
        }
        private Dictionary<string, string> GetCommonHeader()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.1.1; SM-N960F Build/JLS36C; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/74.0.3729.136 Mobile Safari/537.36 MMWEBID/1042 MicroMessenger/7.0.15.1680(0x27000F34) Process/appbrand0 WeChat/arm32 NetType/WIFI Language/zh_CN ABI/arm32");
            headers.Add("Referer", "https://servicewechat.com/wxff8cad2e9bf18719/2/page-frame.html");
            headers.Add("tk", YueMiaoConfig.Instance.Tk);
            headers.Add("Accept", "application/json, text/plain, */*");
            headers.Add("Host", "miaomiao.scmttec.com");
            if (YueMiaoConfig.Instance.Cookies.Count > 0)
            {
                ICollection<string> values = YueMiaoConfig.Instance.Cookies.Values;
                String cookie = String.Join("; ", YueMiaoConfig.Instance.Cookies.Values);
                //   XTrace.WriteLine($"cookie is {cookie}");
                headers.Add("Cookie", cookie);
            }
            return headers;
        }

        private String EccHs(String seckillId, String st)
        {
            String salt = "ux$ad70*b";
            int memberId = YueMiaoConfig.Instance.MemberId;
            String md5 = (seckillId + memberId + st).MDString();
            return (md5 + salt).MDString();
        }
    }
}
