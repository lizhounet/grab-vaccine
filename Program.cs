using grab_vaccine.conf;
using grab_vaccine.exception;
using grab_vaccine.model;
using grab_vaccine.service;
using grab_vaccine.utils;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Security;
using Microsoft.Extensions.Configuration;
using NewLife.Log;
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
            try
            {
                while (true)
                {
                    XTrace.WriteLine("请选择操作类型：1=抢苗，2=查询疫苗，q=退出程序");
                    string opType = Console.ReadLine();
                    switch (opType)
                    {
                        case "1":
                            SecKillVaccine();
                            break;
                        case "2":
                            QueryVaccine();
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

            XTrace.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(YueMiaoConfig.Instance));
            XTrace.WriteLine("接种人员：");
            XTrace.WriteLine($"人员id：{YueMiaoConfig.Instance.MemberId},姓名：{YueMiaoConfig.Instance.MemberName},身份证：{YueMiaoConfig.Instance.IdCard}");
            XTrace.WriteLine("疫苗信息：");
            XTrace.WriteLine($"疫苗id：{YueMiaoConfig.Instance.Vaccine.Id},疫苗名称：{YueMiaoConfig.Instance.Vaccine.VaccineName}，疫苗地址：{YueMiaoConfig.Instance.Vaccine.Address}");
            XTrace.WriteLine("请核实以上信息是否正确！！！！");
            XTrace.WriteLine("回车键启动秒杀!!!!");
            Console.ReadKey();
            seckillService.StartSecKill();
        }
    }
}
