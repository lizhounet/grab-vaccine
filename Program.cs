using grab_vaccine.conf;
using grab_vaccine.exception;
using grab_vaccine.model;
using grab_vaccine.service;
using NewLife.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace grab_vaccine
{
    class Program
    {
        private static readonly GrabSeckillService seckillService = new GrabSeckillService();
        static void Main(string[] args)
        {
            XTrace.UseConsole();
            //RestClient client = new RestClient("https://www.89ip.cn/api.html");
            // client.Proxy = new System.Net.WebProxy("153.122.72.63:80");
            //var request = new RestRequest("", Method.GET);
            //IRestResponse restResponse = client.Execute(request);
            //mXTrace.WriteLine($"验证代理ip结果：{restResponse.Content}");

            //ProxyIpPoolService proxyIpPool = new ProxyIpPoolService();
            //List<string> proxyIpInfos = proxyIpPool.AllTest();
            //proxyIpInfos.ForEach(proxyIp =>
            //{
            //    try
            //    {
            //        XTrace.WriteLine($"当前代理ip：{proxyIp}");
            //        RestClient client = new RestClient("http://icanhazip.com/");
            //        client.Proxy = new System.Net.WebProxy(proxyIp);
            //        var request = new RestRequest("", Method.GET);
            //        IRestResponse restResponse = client.Execute(request);
            //        XTrace.WriteLine($"验证代理ip结果：{restResponse.Content}");
            //    }
            //    catch (Exception ex)
            //    {
            //        XTrace.WriteException(ex);
            //    }
            //});
            //Console.ReadKey();
           
            try
            {
                while (true)
                {
                    XTrace.WriteLine("请选择操作类型：1=抢苗，2=查询疫苗，3=设置抢苗信息，q=退出程序");
                    string opType = Console.ReadLine();
                    switch (opType)
                    {
                        case "1":
                            SecKillVaccine();
                            break;
                        case "2":
                            QueryVaccine();
                            break;
                        case "3":
                            SetYueMiaoConfig();
                            break;
                        case "q":
                            return;
                        default:
                            XTrace.WriteLine("输入无效！！！！");
                            break;
                    }
                }
            }
            catch (BusinessException ex)
            {
                XTrace.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            Console.ReadKey();
        }

        static void QueryVaccine()
        {
            XTrace.WriteLine("请输入查询省份(输入 all 查询平台所有疫苗)");
            string provinceName = Console.ReadLine();
            List<VaccineInfo> vaccineInfos = seckillService.GetVaccineList(provinceName);
            XTrace.WriteLine("疫苗结果：");
            if (vaccineInfos != null && vaccineInfos.Count > 0)
            {
                vaccineInfos.ForEach(vaccine =>
                {
                    XTrace.WriteLine($"{vaccine.Id}-{vaccine.VaccineName}-{vaccine.Name}-{vaccine.Address}-{vaccine.StartTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                });
            }
            else
            {
                XTrace.WriteLine($"{provinceName}-没有疫苗：");
            }
        }
        static void SecKillVaccine()
        {
            //  XTrace.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(YueMiaoConfig.Instance));
            XTrace.WriteLine("接种人员：");
            XTrace.WriteLine($"人员id：{YueMiaoConfig.Instance.MemberId},姓名：{YueMiaoConfig.Instance.MemberName},身份证：{YueMiaoConfig.Instance.IdCard}");
            XTrace.WriteLine("疫苗信息：");
            XTrace.WriteLine($"疫苗id：{YueMiaoConfig.Instance.Vaccine.Id},疫苗名称：{YueMiaoConfig.Instance.Vaccine.VaccineName}，疫苗地址：{YueMiaoConfig.Instance.Vaccine.Address},开始时间：{YueMiaoConfig.Instance.Vaccine.StartTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            XTrace.WriteLine("请核实以上信息是否正确！！！！");
            XTrace.WriteLine("回车键启动秒杀!!!!");
            Console.ReadKey();
            seckillService.StartSecKill();
        }

        static void SetYueMiaoConfig()
        {
            HttpService httpService = new HttpService();
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            {
                List<Member> members = httpService.GetMembers();
                XTrace.WriteLine("设置接种人员，请输入编号后按回车键");
                members.ForEach(member =>
                {
                    XTrace.WriteLine($"编号:{members.IndexOf(member)};姓名:{member.Name}");
                });
                int setMemberIndex = Convert.ToInt32(Console.ReadLine());
                Member member = members[setMemberIndex];
                JObject jsonObject;
                using (StreamReader file = new StreamReader(filePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    jsonObject = (JObject)JToken.ReadFrom(reader);
                    jsonObject["YueMiaoConfig"]["MemberId"] = member.Id;
                    jsonObject["YueMiaoConfig"]["MemberName"] = member.Name;
                    jsonObject["YueMiaoConfig"]["IdCard"] = member.IdCardNo;
                }
                using (var writer = new StreamWriter(filePath))
                using (JsonTextWriter jsonwriter = new JsonTextWriter(writer))
                {
                    jsonObject.WriteTo(jsonwriter);
                }
                XTrace.WriteLine("接种人员设置成功");
            }
            {
                //设置疫苗信息
                XTrace.WriteLine("请输入查询省份(输入 all 查询平台所有疫苗)");
                string provinceName = Console.ReadLine();
                Console.SetCursorPosition(0, Console.CursorTop);
                Task<List<VaccineInfo>> task = new Task<List<VaccineInfo>>(() => seckillService.GetVaccineList(provinceName));
                task.Start();
                for (int i = 0; i <= 100; i++)
                {
                    if (task.IsCompletedSuccessfully) break;
                    Thread.Sleep(1000);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"查询中{i}%");
                }
                List<VaccineInfo> vaccineInfos = task.Result;
                XTrace.WriteLine("疫苗结果：");
                if (vaccineInfos != null && vaccineInfos.Count > 0)
                {
                    XTrace.WriteLine("设置接种人员，请输入编号后按回车键");
                    vaccineInfos.ForEach(vaccine =>
                    {
                        XTrace.WriteLine($"编号:{vaccineInfos.IndexOf(vaccine)}-{vaccine.Id}-{vaccine.VaccineName}-{vaccine.Name}-{vaccine.Address}-{vaccine.StartTime:yyyy-MM-dd HH:mm:ss}");
                    });
                    int setVaccineIndex = Convert.ToInt32(Console.ReadLine());
                    VaccineInfo vaccine = vaccineInfos[setVaccineIndex];
                    JObject jsonObject;
                    using (StreamReader file = new StreamReader(filePath))
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        jsonObject = (JObject)JToken.ReadFrom(reader);
                        jsonObject["YueMiaoConfig"]["Vaccine"]["Id"] = vaccine.Id;
                        jsonObject["YueMiaoConfig"]["Vaccine"]["StartTime"] = vaccine.StartTime;
                        jsonObject["YueMiaoConfig"]["Vaccine"]["VaccineName"] = vaccine.VaccineName;
                        jsonObject["YueMiaoConfig"]["Vaccine"]["Address"] = vaccine.Address;
                    }
                    using (var writer = new StreamWriter(filePath))
                    using (JsonTextWriter jsonwriter = new JsonTextWriter(writer))
                    {
                        jsonObject.WriteTo(jsonwriter);
                    }
                    XTrace.WriteLine("疫苗信息设置成功");
                }
                else
                {
                    XTrace.WriteLine($"{provinceName}-没有疫苗：");
                }
            }
            YueMiaoConfig.ReloadYueMiaoConfig();
        }
    }
}
