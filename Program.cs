using grab_vaccine.exception;
using grab_vaccine.service;
using Masuit.Tools.Security;
using NewLife.Log;
using System;

namespace grab_vaccine
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();
            try
            {
                HttpService httpService = new HttpService();
                httpService.Log("125574");
            }
            catch (BusinessException ex)
            {
                XTrace.WriteException(ex);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            Console.ReadKey();
        }
    }
}
