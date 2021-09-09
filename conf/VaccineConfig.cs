using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.conf
{
    public class VaccineConfig
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

        /**
         * 调用接口时返回的set-cookie
         */
        public Dictionary<string, string> Cookies { set; get; } = new Dictionary<string, string>(10);


        /// <summary>
        /// 单例实现
        /// </summary>
        private static readonly Lazy<VaccineConfig> lazy =
          new Lazy<VaccineConfig>(() =>
          {
              //读取配置文件
              return new VaccineConfig();
          });
        public static VaccineConfig Instance { get { return lazy.Value; } }

        private VaccineConfig() { }
    }
}
