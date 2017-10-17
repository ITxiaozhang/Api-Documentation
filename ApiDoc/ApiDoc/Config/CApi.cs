using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Linq.Expressions;
using System.Collections;
using System.Web.Mvc;
using System.Reflection;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;


namespace CApi
{
    /// <summary>
    /// 作者：zc【2017-10-16】
    /// 联系QQ:947623232【联系请备注:ApiDoc】
    /// </summary>
    public class ApiUtils
    {

        #region 读取配置
        //public static string _DefaultAssembly = "DefaultAssembly".GetAppSettingsStr();//默认程序集
        //public static string _DefaultController = "DefaultController".GetAppSettingsStr();//默认控制器前缀
        //public static string _DefaultNameSpace = "DefaultNameSpace".GetAppSettingsStr();//默认类所在的命名空间
        //public static string _ExNameSpace = "ExNameSpace".GetAppSettingsStr();//默认扩展类所在的命名空间
        //public static string _ApiNameSpace = "ApiNameSpace".GetAppSettingsStr();//默认扩展类所在的命名空间

        //-----出默认程序集外-其他配置的名称最后必须加点【.】
        public static string _DefaultAssembly = "ApiDoc";//默认程序集
        public static string _DefaultController = "ApiDoc.Controllers.";//默认控制器前缀
        public static string _DefaultNameSpace = "ApiDoc.Controllers.";//默认返回实体（BackClassName）所在的命名空间
        public static string _ExNameSpace = "ApiDoc.Controllers.";//默认ex.实体所在的命名空间
        public static string _ApiNameSpace = "CApi.";//默认api.实体所在的命名空间


        public static string _ApiKey = "";
        public static string _ApiRequestUrl = "http://localhost:10001";
        public static string _IndexTitle = "Api文档说明";//首页标题
        public static string _PreName = "Api";//前缀

        public static string _ControllName = "";//控制器名称
        public static string _ActionName = "";//方法名称
        /// <summary>
        /// 自定义的参数描述配置-不用配置FiledType
        /// </summary>
        /// <returns></returns>
        public static List<FiledsData> GetParamFiledList()
        {
            var fDataList = new List<FiledsData>();
            fDataList.Add(new FiledsData("abc", "", 0, "abc的说明文字啊"));
            fDataList.Add(new FiledsData("abc1", "", 0, "abc1的说明文字啊"));
            fDataList.Add(new FiledsData("abc2", "", 0, "abc2的说明文字啊"));
            fDataList.Add(new FiledsData("abc3", "", 0, "abc3的说明文字啊"));
            fDataList.Add(new FiledsData("abc4", "", 0, "abc4的说明文字啊"));
            fDataList.Add(new FiledsData("abc5", "", 0, "abc5的说明文字啊"));
            fDataList.Add(new FiledsData("abc6", "", 0, "abc6的说明文字啊"));
            return fDataList;
        }
        /// <summary>
        /// 自定义的参数描述配置-通过字段名获取字段信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FiledsData GetParamFiledByName(string name)
        {
            return GetParamFiledList().Where(x => x.Name.ToUpper() == name.ToUpper()).FirstOrDefault();
        }

        #endregion

        #region 创建Api的说明文档
        /// <summary>
        /// 创建Api的说明文档
        /// </summary>
        /// <param name="controllerName">控制器名称【不区分大小写】</param>
        /// <param name="actionName">方法名【区分大小写】</param>
        /// <param name="isPreName">是否加前缀</param>
        /// <returns></returns>
        public static List<string> CreateApiFile(string controllerName, string actionName, bool isPreName = true)
        {
            var result = new Dictionary<string, object>();
            try
            {
                result = ApiUtils.GetApiConfigData(controllerName, actionName, isPreName);
                var apiData3 = (ApiData)result.Where(x => x.Key == "BackData").Select(x => x.Value).FirstOrDefault();
                if (apiData3 == null)
                    return result.Select(x => x.Key + ":" + x.Value.ToStr()).ToList();
                var createApiHtmlList = ApiUtils.CreateApiHtml(apiData3);
                if (createApiHtmlList != null && createApiHtmlList.Count > 0)
                    result.Add("CreateApiHtml_Exception", createApiHtmlList);
            }
            catch (Exception ex)
            {
                result.Add("CreateApiFile_Exception", ex.Message);
            }
            return result.Where(x => x.Key != "BackData").Select(x => x.Key + ":" + x.Value).ToList();
        }
        #endregion

        #region 批量-重新生成Api的说明文档-【读取-config.json配置】或者【传入指定页面】

        public static string CreateApiFile_All(List<string> list)
        {
            var i = 0;
            var CApiConfigList = CApiConfig.GetList();
            if (list != null && list.Count > 0)
                CApiConfigList = CApiConfigList.Where(x => list.Contains(x.Url)).ToList();
            foreach (var item in CApiConfigList)
            {
                if (CreateApiFile(item.CName, item.AName, item.IsPre).Count > 0)
                    i++;
            }
            var HtmlStr = "";
            HtmlStr = string.Format("<h2>重新生成完毕！</h2><br>成功：{0}条<br>失败：{1}条", CApiConfigList.Count, i);
            if (CApiConfigList.Count == 0)
                HtmlStr = "<h2>无可生成的文件！</h2>";
            return HtmlStr;
        }
        #endregion

        #region Api接口测试
        /// <summary>
        /// Api接口测试
        /// </summary>
        /// <param name="paramStr"></param>
        /// <param name="apiUrl"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public static string TestApiRequest(string paramStr, string apiUrl, int methods)
        {
            var msg = "";
            try
            {
                #region 生成最终需要传递给接口的参数【方法中已生成Sign】
                var paramDic = ConvertUtils.UrlParamToDic(paramStr);//参数转Dictionary
                var sign = MakeSign(paramDic);
                //paramDic.Add("APIKEY", sign);
                #endregion
                if (methods == 0 || methods == 1)
                    msg = apiUrl.ToAppend("?" + paramDic.ToUrlParam()).SendGet();
                else
                    msg = apiUrl.SendPost(paramDic.ToUrlParam());
            }
            catch (Exception)
            {
                msg = "Error";
            }
            return msg;
        }
        #endregion

        #region 签名方法-示例
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string MakeSign(Dictionary<string, string> dic)
        {
            var appKey = "AMSToHR-2017";
            var signStr = dic.OrderBy(q => q.Key).Aggregate("", (c, q) => c + dic[q.Key] + "|");
            var str = ConvertUtils.ToMD5(signStr + appKey);
            return str;
        }
        #endregion


        #region 获取方法配置对应数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cName">控制器名称</param>
        /// <param name="aName">方法名</param>
        /// <param name="isPreName">是否加前缀</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetApiConfigData(string cName, string aName, bool isPreName)
        {
            var resultList = new Dictionary<string, object>();
            _ControllName = cName + "Controller";
            _ActionName = aName;
            var preStr = isPreName ? _PreName : "";
            var apiData = new ApiData();
            try
            {
                Assembly asse = Assembly.Load(_DefaultAssembly);
                Type type = asse.GetType(_DefaultController + _ControllName, false, true);
                if (type == null)
                    resultList.Add("ControllName", "控制器【" + _ControllName + "】未找到.");
                var index = type.GetMethod(_ActionName);
                if (index == null)
                    resultList.Add("ActionName", "方法【" + _ActionName + "】未找到.");
                var param = index.GetParameters();
                var attrs = index.GetCustomAttributes().ToList();
                //var backList = index.GET();
                var paramFDList = new List<FiledsData>();
                foreach (var item in param)
                {
                    #region 添加传入参数
                    if (item.ParameterType.IsValueType || item.ParameterType == typeof(string))
                    {
                        var pFD = GetParamFiledByName(item.Name);
                        pFD.FiledType = GetFieldsType(item.ParameterType);
                        paramFDList.Add(pFD);
                    }
                    else
                    {
                        var getItem = item.ParameterType;
                        var FiledsDataList = GetFiledsDataList(getItem);
                        paramFDList.AddRange(FiledsDataList);
                    }
                    #endregion
                }
                apiData.IsPre = isPreName;
                apiData.CName = cName.ToStr();
                apiData.ParamDataList = paramFDList;
                apiData.Methods = "Get/Post";
                apiData.FuncName = _ActionName.ToStr();
                var url = "/" + cName + "/" + _ActionName;
                preStr = "/" + preStr.Replace(" ", "");
                apiData.ApiUrl = preStr.Length == 1 ? url : (preStr + url);

                #region 通过特性查找各种属性值
                for (int i = 0; i < attrs.Count(); i++)
                {
                    if (attrs[i].GetType() == typeof(ApiConfigAttribute))
                    {
                        var t1 = (ApiConfigAttribute)attrs[i];
                        apiData.Describtion = t1.Describtion.ToStr();
                        apiData.Name = t1.Name.ToStr();
                        #region 添加-返回参数
                        var bcName = t1.BackClassName.ToStr();
                        if (!string.IsNullOrEmpty(bcName))
                        {
                            if (bcName.ToUpper().IndexOf("EX.") >= 0)
                                bcName = bcName.Replace("ex.", _ExNameSpace);
                            else if (bcName.ToUpper().IndexOf("API.") >= 0)
                                bcName = bcName.Replace("api.", _ApiNameSpace);
                            else
                                bcName = _DefaultNameSpace + bcName;
                            var bcType = asse.GetType(bcName);
                            var FiledsDataList = GetFiledsDataList(bcType);
                            apiData.BackDataList = FiledsDataList;
                        }
                        else
                            apiData.BackDataList = null;
                        #endregion
                    }
                    else if (attrs[i].GetType() == typeof(HttpGetAttribute))
                        apiData.Methods = "Get";
                    else if (attrs[i].GetType() == typeof(HttpPostAttribute))
                        apiData.Methods = "Post";
                }
                #endregion
                resultList.Add("BackData", apiData);
            }
            catch (Exception ex)
            {
                resultList.Add("GetApiConfigData_Exception", ex.Message.ToStr());
            }
            return resultList;
        }
        #endregion

        #region 传入类型-返回类型对应字段的详情说明对象
        public static List<FiledsData> GetFiledsDataList(Type bcType)
        {
            var FiledsDataList = new List<FiledsData>();
            foreach (var bcItem in bcType.GetProperties())
            {
                if (!bcItem.PropertyType.IsValueType && bcItem.PropertyType != typeof(string))
                    continue;
                var fd = new FiledsData();
                fd.Name = bcItem.Name;
                fd.FiledType = GetFieldsType(bcItem.PropertyType);
                var bcAttrs = bcItem.GetCustomAttributes().ToList();
                for (int k = 0; k < bcAttrs.Count(); k++)
                {
                    if (bcAttrs[k].GetType() == typeof(RemarkAttribute))
                    {
                        fd.IsWrite = ((RemarkAttribute)bcAttrs[k]).GetData[1].ToInt();
                        fd.Remark = ((RemarkAttribute)bcAttrs[k]).GetData[0].ToStr();
                    }
                }
                FiledsDataList.Add(fd);
            }
            return FiledsDataList;
        }
        #endregion

        #region 获取对应字段类型
        public static string GetFieldsType(Type t)
        {
            if (t == typeof(string))
                return "string";
            else if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64))
                return "int";
            else if (t == typeof(decimal) || t == typeof(Double) || t == typeof(float) || t == typeof(double))
                return "float";
            else if (t == typeof(bool) || t == typeof(Boolean))
                return "bool";
            else if (t == typeof(DateTime))
                return "datetime";
            else
                return "";
        }
        #endregion

        #region 模版读取-将动态数据写入模版
        #region 创建-单个Html
        public static List<string> CreateApiHtml(ApiData apiData)
        {
            var resultList = new List<string>();
            try
            {
                var jsonHelper = new JavaScriptSerializer();
                apiData.EntityNullToStr<ApiData>();//对象中的string类型空值转成""
                var html = File.ReadAllText(CApiConfig.templateUrl);
                var paramHtml = GetFiledsHtml(apiData.ParamDataList, 1);
                var backparamHtml = GetFiledsHtml(apiData.BackDataList, 0, true);
                var dataDic = new Dictionary<string, object>();
                foreach (var item in apiData.BackDataList)
                    dataDic.Add(item.Name, "");
                var dic = new Dictionary<string, object>() { };
                dic.Add("Stata", "200");
                dic.Add("Message", "ok");
                dic.Add("Data", dataDic);
                var backSuccess = jsonHelper.Serialize(dic);
                #region 替换文本
                html = html.Replace("{{ParamDataList}}", paramHtml);
                html = html.Replace("{{BackDataList}}", backparamHtml);
                html = html.Replace("{{Name}}", apiData.Name);
                html = html.Replace("{{BackSuccess}}", backSuccess);
                html = html.Replace("{{Describtion}}", apiData.Describtion);
                html = html.Replace("{{ApiUrl}}", apiData.ApiUrl);
                html = html.Replace("{{Methods}}", apiData.Methods);
                html = html.Replace("{{FuncName}}", apiData.FuncName);
                #endregion

                #region 生成文件-html
                string path = CApiConfig.fileUrl;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string fileName = string.Format("{0}{1}.html", apiData.CName, "_" + apiData.FuncName);
                string fileFullPath = CApiConfig.fileUrl + fileName;
                StreamWriter sw;
                if (!File.Exists(fileFullPath))
                {
                    sw = File.CreateText(fileFullPath);
                    sw.WriteLine(html.ToString());
                    sw.Close();
                }
                else
                    File.WriteAllText(fileFullPath, html);
                #endregion

                #region 保存生成的记录-config.json
                var list = CApiConfig.GetList() ?? new List<CApiConfig>();
                list.Add(new CApiConfig(apiData.CName, apiData.Name, fileName, apiData.FuncName, apiData.IsPre));
                list = list.GroupBy(x => x.Url).Select(x => x.FirstOrDefault()).ToList();//去重复
                CApiConfig.SaveData(list);
                #endregion

                #region 生成文件-Index文件
                var IndexHtml = File.ReadAllText(CApiConfig.templateUrl_index);//读取模板文件
                var one_Url = list == null ? "" : list[0].Url;
                var one_Name = list == null ? "" : list[0].Name;
                foreach (var item in list.GroupBy(x => x.CName).Select(x => x.FirstOrDefault()).ToList())
                {
                    var sbHtml = new StringBuilder();
                    var i = 0;
                    foreach (var a in list.Where(x => x.CName == item.CName))
                    {
                        sbHtml.AppendFormat(aHtml, a.Url, a.Name, i++);
                    }
                    IndexHtml = IndexHtml.Replace("{{" + item.CName + "}}", sbHtml.ToString());
                }
                #region 替换配置信息
                IndexHtml = IndexHtml.Replace("{{One_Url}}", one_Url);
                IndexHtml = IndexHtml.Replace("{{One_Name}}", one_Name);
                IndexHtml = IndexHtml.Replace("{{IndexTitle}}", _IndexTitle);
                IndexHtml = IndexHtml.Replace("{{Api_Url}}", _ApiRequestUrl);
                #endregion
                string IndexFileFullPath = CApiConfig.indexUrl;
                File.WriteAllText(IndexFileFullPath, IndexHtml);
                #endregion
            }
            catch (Exception ex)
            {
                resultList.Add(ex.Message);
            }
            return resultList;
        }
        #endregion

        #region 每个Html标签块
        private static string trHtml = @"<tr>
                                            <td>{4}{0}</td>
                                            <td>{1}</td>
                                            <td>{2}</td>
                                            <td>{3}</td>
                                        </tr>";
        private static string aHtml = "<a class=\"J_menuItem\" href=\"{0}\"  data-index=\"{2}\">{1}</a>";
        #endregion

        #region 多行行单元格Html
        /// <summary>
        /// 生成多行单元格的Html
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type">1：生成空的一行TR(内容为--)   0：不生成空TR</param>
        /// <param name="isBack">是否为返回参数</param>
        /// <returns></returns>
        public static string GetFiledsHtml(List<FiledsData> list, int type = 0, bool isBack = false)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.AppendFormat(trHtml, item.Name, item.FiledType, item.IsWrite == 1 ? "(必填)" : "", item.Remark, (item.IsWrite == 1 && !isBack) ? "<font>*</font>" : "");
            if (string.IsNullOrEmpty(sb.ToStr()) && type == 1)
                sb.AppendFormat(trHtml, "--", "--", "--", "--", "");
            return sb.ToString();
        }
        #endregion



        #endregion
    }

    #region 方法-特性属性
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiConfigAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 详细说明
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 返回类型的对象路径
        /// 例:ApiDoc.Controllers.Student
        /// </summary>
        public string BackClassName { get; set; }
    }
    #endregion

    #region 字段--特性属性
    [AttributeUsage(AttributeTargets.All)]
    public class RemarkAttribute : Attribute
    {
        /// <summary>
        /// 说明文字|1    【1:必填   非0:选填】
        /// </summary>
        /// <param name="remark"></param>
        public RemarkAttribute(string remark = "")
        {
            if (remark.IndexOf("|") < 0)
                remark += "|";
            var arr = remark.Split('|').ToList();
            this.GetData = arr;
        }
        public List<string> GetData { get; set; }
    }
    #endregion

    #region 字段属性说明的对象
    public class FiledsData
    {
        public FiledsData() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="filedType">字段类型string,int,bool</param>
        /// <param name="isWrite">是否必填 1:必填   非0:选填</param>
        /// <param name="remark">备注/说明</param>
        public FiledsData(string name, string filedType, int isWrite, string remark)
        {
            this._Name = name;
            this._Remark = remark;
            this._IsWrite = isWrite;
            this._FiledType = filedType;
        }
        private string _Name = "";
        private string _FiledType = "";
        private int _IsWrite = 0;
        private string _Remark = "";

        [Remark("名称|1")]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        [Remark("类型")]
        public string FiledType
        {
            get { return _FiledType; }
            set { _FiledType = value; }
        }
        /// <summary>
        /// 1:必填   非0:选填
        /// </summary>
        [Remark("是否必填")]
        public int IsWrite
        {
            get { return _IsWrite; }
            set { _IsWrite = value; }
        }
        [Remark("备注")]
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
        }
    }
    #endregion

    #region 返回特性配置读取结果对象
    public class ApiData
    {
        public List<FiledsData> ParamDataList { get; set; }
        public List<FiledsData> BackDataList { get; set; }
        public string ApiUrl { get; set; }
        public string Name { get; set; }
        public bool IsPre { get; set; }
        public string CName { get; set; }
        public string FuncName { get; set; }
        public string Methods { get; set; }
        public string Describtion { get; set; }
        public string BackSuccess { get; set; }

    }
    #endregion

    #region 配置-记录生成的文件的json
    /// <summary>
    /// CApiConfig 的摘要说明
    /// </summary>
    public class CApiConfig
    {
        public CApiConfig() { }
        public CApiConfig(string _CName, string _Name, string _Url, string _AName = "", bool _IsPre = true)
        {
            this.CName = _CName;
            this.AName = _AName;
            this.Name = _Name;
            this.Url = _Url;
            this.IsPre = _IsPre;
        }
        public static string fileUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\";
        public static string configUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\config.json";
        public static string templateUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\api-tmp.html";
        public static string templateUrl_index = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\Config\\index-tmp.html";
        public static string indexUrl = AppDomain.CurrentDomain.BaseDirectory + "ApiDoc\\index.html";
        public static List<CApiConfig> GetList()
        {
            var json = File.ReadAllText(configUrl);
            var sear = new JavaScriptSerializer();
            return sear.Deserialize<List<CApiConfig>>(json);
        }

        public static void SaveData(List<CApiConfig> list)
        {
            var sear = new JavaScriptSerializer();
            string json = sear.Serialize(list);
            StreamWriter sw;
            if (!File.Exists(configUrl))
            {
                sw = File.CreateText(configUrl);
                sw.WriteLine(json.ToString());
                sw.Close();
            }
            else
                File.WriteAllText(configUrl, json);

        }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string CName { get; set; }
        /// <summary>
        /// 方法名
        /// </summary>
        public string AName { get; set; }
        /// <summary>
        /// 方法显示名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件名  *.html
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 是否有前缀
        /// </summary>
        public bool IsPre { get; set; }
    }
    #endregion


    #region 常用扩展-
    public static class ConvertUtils
    {

        #region 模拟Get请求方法
        /// <summary>
        /// Get获取请求结果
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SendGet(this string url)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            string str = "";
            using (WebResponse wr = request.GetResponse())
            {
                response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                str = reader.ReadToEnd();
            }
            return str;
        }
        #endregion

        #region 模拟Post请求方法
        /// <summary>
        /// Post获取请求结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string SendPost(this string strURL, string param)
        {
            HttpWebRequest request = WebRequest.Create(strURL) as HttpWebRequest;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(param);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream()) { stream.Write(data, 0, data.Length); }
            var sm = (request.GetResponse() as HttpWebResponse).GetResponseStream();
            StreamReader sr = new StreamReader(sm, Encoding.UTF8);
            return sr.ReadToEnd();
        }
        #endregion

        #region 转成Url传参格式 a=1&b=3&c=3
        /// <summary>
        /// 转成Url传参格式 a=1&b=3&c=3
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string ToUrlParam(this Dictionary<string, string> dic)
        {
            var list = dic.Select(x => new { k = x.Key + "=" + x.Value }).Select(x => x.k).ToList();
            var str = string.Join("&", list);
            return str;
        }
        #endregion

        #region 转成Dictionary<string,string>原字符 a=1&b=3&c=3
        /// <summary>
        ///  转成Dictionary<string,string>字符 a=1&b=3&c=3
        /// </summary>
        /// <param name="paramStr"></param>
        /// <returns></returns>
        public static Dictionary<string, string> UrlParamToDic(string paramStr)
        {
            var dic = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(paramStr))
                return dic;
            var paramArr = paramStr.Split('&');
            for (int i = 0; i < paramArr.Length; i++)
                dic.Add(paramArr[i].Split('=')[0], paramArr[i].Split('=')[1]);
            return dic;
        }
        #endregion

        #region 字符串MD5+Key 加密
        /// <summary>
        /// 字符串MD5+Key 加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToMD5(string data)
        {
            StringBuilder sb = new StringBuilder();
            MD5 md5 = new MD5CryptoServiceProvider();
            var Key = ""; //读取配置文件
            byte[] t = md5.ComputeHash(Encoding.UTF8.GetBytes("32333" + data + "z32!"));
            foreach (var t1 in t)
            {
                sb.Append(t1.ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }
        #endregion

        #region 获取配置的内容
        /// <summary>
        /// 获取配置中的内容-为空择返回""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetAppSettingsStr(this string str)
        {
            return ConfigurationManager.AppSettings[str].ToStr();
        }
        #endregion

        #region 数组拼接成字符串
        /// <summary>
        /// 数组拼接成字符串
        /// </summary>
        /// <param name="ienumerable"></param>
        /// <param name="splitStr">分割字符串:默认为逗号</param>
        /// <returns></returns>
        public static string ToJoinStr(this IEnumerable<object> ienumerable, string splitStr = ",")
        {
            return string.Join(splitStr, ienumerable);
        }
        #endregion

        #region 将后面字符串追加到当前字符后面
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="newStr"></param>
        /// <returns></returns>
        public static string ToAppend(this string str, string newStr)
        {
            var sb = new StringBuilder();
            sb.Append(str);
            sb.Append(newStr);
            return sb.ToStr();
        }
        #endregion


        public static string ToStr(this object value, string defaultValue = "")
        {
            if (value == null) return defaultValue;
            else
            {
                return value.ToString();
            }
        }
        public static int ToInt(this object value, int defaultValue = 0)
        {
            int rst;
            if (value == null) return defaultValue;
            if (int.TryParse(value.ToString(), out rst))
            {
                return rst;
            }
            else
            {
                return defaultValue;
            }
        }

        #region 实体--有Null的string的成员变量改为""
        /// <summary>
        /// 实体--有Null的string的成员变量改为""
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T EntityNullToStr<T>(this T self)
        {
            try
            {
                foreach (PropertyInfo mItem in typeof(T).GetProperties())
                {
                    var mItemVal = mItem.GetValue(self, new object[] { });
                    if (mItem.PropertyType == typeof(string))
                    {
                        mItem.SetValue(self, mItemVal.ToStr(), null);
                    }
                }
            }
            catch (NullReferenceException NullEx)
            {
                throw NullEx;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            return self;
        }
        #endregion
    }
    #endregion
}
