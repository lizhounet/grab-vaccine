using grab_vaccine.conf;
using grab_vaccine.exception;
using grab_vaccine.model;
using grab_vaccine.utils;
using Masuit.Tools.DateTimeExt;
using NewLife.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace grab_vaccine.service
{
    /// <summary>
    /// 疫苗秒杀Service
    /// </summary>
    public class GrabSeckillService
    {
        private readonly HttpService httpService = new HttpService();
        private readonly ProxyIpPoolService ipPoolService = new ProxyIpPoolService();
        /// <summary>
        /// 开启秒杀
        /// </summary>
        public void StartSecKill()
        {

            model.VaccineInfo vaccine = YueMiaoConfig.Instance.Vaccine;
            //疫苗开始时间(提前500ms，开始抢苗)
            DateTime startTime = vaccine.StartTime.AddMilliseconds(-500);
            string st = string.Empty;
            //代理ip池
            List<ProxyIpInfo> proxyIpInfos = new List<ProxyIpInfo>();
            TimeSpan timeSpan = default;
            //还没到时间，等待
            while (DateTime.Now < startTime)
            {
                TimeSpan timeSpan1 = (startTime - DateTime.Now);
                if (timeSpan1.Seconds == timeSpan.Seconds)
                {
                    //一秒内跳过
                    continue;
                }
                else
                {
                    timeSpan = timeSpan1;
                }
                XTrace.WriteLine($"疫苗{vaccine.Id}-{vaccine.VaccineName}-{vaccine.Address}还未到开始时间，等待中,剩余：{timeSpan.Days}天{timeSpan.Hours}小时{timeSpan.Minutes}分{timeSpan.Seconds}秒");
                Thread.Sleep(100);
                if (timeSpan.Seconds < 30 && timeSpan.Minutes <= 0)
                {
                    //提前30s获取代理ip
                    if (proxyIpInfos.Count <= 0)
                    {
                        //获取代理ip
                        proxyIpInfos = ipPoolService.All("https");
                    }
                    //if (string.IsNullOrEmpty(st))
                    //{
                    //    //提前一分钟获取st
                    //    try
                    //    {
                    //        st = httpService.GetSt(vaccine.Id.ToString());
                    //        XTrace.WriteLine($"获取st成功:{st}");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        XTrace.WriteException(ex);
                    //        st = DateTime.Now.GetTotalMilliseconds().ToString();
                    //        XTrace.WriteLine($"获取st异常,准备使用本地时间戳:{st}");
                    //    }
                    //}
                }
            }
            if (proxyIpInfos.Count <= 0)
            {
                //获取代理ip
                proxyIpInfos = ipPoolService.All("https");
            }
            string orderId = string.Empty;
            //   proxyIpInfos = ipPoolService.AllTest().Select(t => new ProxyIpInfo() { proxy = t }).ToList();
            XTrace.WriteLine($"可用代理ip数量：{proxyIpInfos.Count}个");
            //如果没有代理ip 则不使用代理ip 使用少量线程秒杀(2-3个线程)
            if (proxyIpInfos.Count <= 0)
            {
                Parallel.For(0, 3, (index) =>
                {
                    do
                    {
                        try
                        {
                            st = DateTime.Now.GetTotalMilliseconds().ToString();
                            orderId = httpService.secKill(vaccine.Id.ToString(), "1", YueMiaoConfig.Instance.MemberId.ToString(),
                               YueMiaoConfig.Instance.IdCard, st);
                            if (!string.IsNullOrEmpty(orderId))
                            {
                                XTrace.WriteLine("抢购成功");
                                break;
                            }
                        }
                        catch (BusinessException bex)
                        {
                            XTrace.WriteLine(bex.Message);
                        }
                        catch (Exception ex)
                        {
                            XTrace.WriteException(ex);
                        }
                        finally
                        {
                            Thread.Sleep(400);
                        }
                    }
                    while (string.IsNullOrEmpty(orderId));
                });
            }
            else
            {
                //添加两个本地ip抢
                proxyIpInfos.Add(new ProxyIpInfo());
                proxyIpInfos.Add(new ProxyIpInfo());
                Parallel.ForEach(proxyIpInfos, (proxyIpInfo) =>
                {
                    do
                    {
                        try
                        {
                            st = DateTime.Now.GetTotalMilliseconds().ToString();
                            System.Net.WebProxy webProxy = null;
                            if (!string.IsNullOrEmpty(proxyIpInfo.proxy))
                            {
                                webProxy = new System.Net.WebProxy(proxyIpInfo.proxy);
                            }
                            orderId = httpService.secKill(vaccine.Id.ToString(), "1", YueMiaoConfig.Instance.MemberId.ToString(),
                               YueMiaoConfig.Instance.IdCard, st, webProxy);
                            if (!string.IsNullOrEmpty(orderId))
                            {
                                XTrace.WriteLine("抢购成功");
                                break;
                            }
                        }
                        catch (BusinessException bex)
                        {
                            XTrace.WriteLine(bex.Message);
                        }
                        catch (Exception ex)
                        {
                            XTrace.WriteException(ex);
                        }
                        finally
                        {
                            Thread.Sleep(500);
                        }
                    }
                    while (string.IsNullOrEmpty(orderId));
                });
            }
            XTrace.WriteLine(string.IsNullOrEmpty(orderId) ? "抢购失败" : "抢购成功，请登录约苗小程序查看");

        }
        /// <summary>
        /// 获取疫苗列表
        /// </summary>
        /// <param name="provinceName">省份名称,默认扫描平台所有疫苗</param>
        /// <returns></returns>
        public List<VaccineInfo> GetVaccineList(string provinceName = "all")
        {
            ConcurrentBag<VaccineInfo> vaccineInfos = new ConcurrentBag<VaccineInfo>();
            if ("all".Equals(provinceName))
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(4, 4);
                ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
                XTrace.WriteLine($"线程池中辅助线程的最小数目：{minWorkerThreads} 线程池中异步 I/O 线程的最大数目：{minCompletionPortThreads}");
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                XTrace.WriteLine($"线程池中辅助线程的最小数目：{maxWorkerThreads} 线程池中异步 I/O 线程的最大数目：{maxCompletionPortThreads}");
                Parallel.ForEach(ParseUtil.GetAreas(), (area) =>
                {
                    ParseUtil.GetChildren(area.Name).ForEach(city =>
                    {
                        //休息100ms 请求太频繁了 约苗要限制
                        Thread.Sleep(100);
                        List<VaccineInfo> tempVaccineInfos = httpService.GetVaccineList(city.Value);
                        if (tempVaccineInfos != null && tempVaccineInfos.Count > 0)
                        {
                            XTrace.WriteLine($"{area.Name}-{city.Name}有疫苗");
                            tempVaccineInfos.ForEach(t => vaccineInfos.Add(t));
                        }
                    });
                });
            }
            else
            {
                ParseUtil.GetChildren(provinceName).ForEach(city =>
                {
                    List<VaccineInfo> tempVaccineInfos = httpService.GetVaccineList(city.Value);
                    if (tempVaccineInfos != null && tempVaccineInfos.Count > 0)
                    {
                        XTrace.WriteLine($"{provinceName}-{city.Name}有疫苗");
                        tempVaccineInfos.ForEach(t => vaccineInfos.Add(t));
                    }
                });
            }

            return vaccineInfos.ToList();
        }
    }
}
