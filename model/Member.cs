using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.model
{
    /// <summary>
    /// 接种人信息
    /// </summary>
    public class Member
    {
        public int Id { set; get; }

        /**
         * 姓名
         */
        public String Name { set; get; }

        /**
         * 身份证号码
         */
        public String IdCardNo { set; get; }
    }
}
