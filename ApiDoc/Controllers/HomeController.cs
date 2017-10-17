using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.EnterpriseServices;
using CApi;

namespace ApiDoc.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 作者：zc【2017-10-16】
        /// 联系QQ:947623232【联系请备注:ApiDoc】
        /// </summary>
        public HomeController()
        {

        }
        //
        // GET: /Home/
        public ActionResult Index()
        {
            ///测试调用-生成3个接口对应的api文档-调用后会记录到Config.json文件中,用于重新生成的数据配置和菜单读取使用
            ApiUtils.CreateApiFile("Home", "GetName", false);
            ApiUtils.CreateApiFile("Home", "GetName2", false);
            ApiUtils.CreateApiFile("Home", "GetName3", false);
            ApiUtils.CreateApiFile("User", "GetUser", false);
            return View();
        }


        public ActionResult Index2(string Path = "")
        {
            ///测试调用-生成指定地址对应的api文档-无参数，择生成所有文件（通过/apidoc/config/config.json 来生成文件）
            var list = new List<string>();
            if (!string.IsNullOrEmpty(Path))
                list.Add(Path);
            var html = ApiUtils.CreateApiFile_All(list);
            ViewBag.HtmlStr = html;
            return View();
        }


        [HttpGet]
        [ApiConfig(Name = "获取姓名", Describtion = "我是获取姓名的详细的说明...", BackClassName = "api.FiledsData")]
        public JsonResult GetName(string abc, Student stu)
        {
            var list = new List<FiledsData>();
            list.Add(new FiledsData() { Name = "Name1", IsWrite = 1, Remark = "remark1" });
            list.Add(new FiledsData() { Name = "Name2", IsWrite = 0, Remark = "remark2" });
            return Json(new { Status = 200, Message = "ok", Data = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ApiConfig(Name = "获取姓名2", Describtion = "2我是获取姓名的详细的说明2...", BackClassName = "Student")]
        public JsonResult GetName2(string abc, Student stu)
        {
            var list = new List<Student>();
            list.Add(new Student() { Age = 11, Name = "张三2" });
            list.Add(new Student() { Age = 8, Name = "李四2" });
            list.Add(new Student() { Age = 12, Name = "王五2" });
            return Json(new { Status = 200, Message = "ok", Data = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ApiConfig(Name = "获取姓名3", Describtion = "3我是获取姓名的详细的说明3...", BackClassName = "ex.Student")]
        public JsonResult GetName3(string abc, Student stu)
        {
            var list = new List<Student>();
            list.Add(new Student() { Age = 11, Name = "张三3" });
            list.Add(new Student() { Age = 8, Name = "李四3" });
            list.Add(new Student() { Age = 12, Name = "王五3" });
            return Json(new { Status = 200, Message = "ok", Data = list }, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// 接收模拟请求接口
        /// </summary>
        /// <param name="paramStr"></param>
        /// <param name="apiUrl"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public JsonResult TestApiRequest(string paramStr, string apiUrl, int methods)
        {
            var msg = ApiUtils.TestApiRequest(paramStr, apiUrl, methods);
            if (msg == "Error")
                return Json(new { Status = 500, Message = "请求失败,请检查参数!" }, JsonRequestBehavior.AllowGet);
            return Json(new { Status = 200, Message = "ok", Data = msg }, JsonRequestBehavior.AllowGet);
        }
    }



    public class Student
    {
        [Remark("姓名|1")]
        public string Name { get; set; }
        [Remark("年龄")]
        public int Age { get; set; }
    }
}
