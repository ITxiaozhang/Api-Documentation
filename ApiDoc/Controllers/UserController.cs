using CApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ApiDoc.Controllers
{
    public class UserController : Controller
    {
        /// <summary>
        /// 作者：zc【2017-10-16】
        /// 联系QQ:947623232【联系请备注:ApiDoc】
        /// </summary>
        public UserController()
        {

        }

        [HttpGet]
        [ApiConfig(Name = "用户相关", Describtion = "我是获取用户相关的详细的说明...", BackClassName = "api.FiledsData")]
        public JsonResult GetUser(Student stu)
        {
            var list = new List<FiledsData>();
            list.Add(new FiledsData() { Name = "用户相关1", IsWrite = 1, Remark = "remark1" });
            list.Add(new FiledsData() { Name = "用户相关2", IsWrite = 0, Remark = "remark2" });
            return Json(new { Status = 200, Message = "ok", Data = list }, JsonRequestBehavior.AllowGet);
        }


    }
}
