using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.model
{
    /// <summary>
    /// 约苗疫苗信息
    /// </summary>
    public class VaccineInfo
    {
        /// <summary>
        /// 疫苗id
        /// </summary>
        public int Id { set; get; }
        /// <summary>
        /// 医院名称
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 医院地址
        /// </summary>
        public string Address { set; get; }
        /// <summary>
        /// 疫苗代码
        /// </summary>
        public string VaccineCode { set; get; }
        /// <summary>
        /// 疫苗名称
        /// </summary>
        public string VaccineName { set; get; }
        /// <summary>
        /// 疫苗开抢时间
        /// </summary>
        public DateTime StartTime { set; get; }
    }
}
