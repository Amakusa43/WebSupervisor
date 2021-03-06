﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using WebSupervisor.Models;
using WebDAL;
using PagedList;
using WebSupervisor.Code.Placement;
using WebSupervisor.Controllers.CheckUser;
using WebSupervisor.Code.Classes;
using System.IO;
using System.Text;

namespace WebSupervisor.Controllers
{
    [AuthenAdmin]
    public class SupervisorController : Controller
    {
        List<TeachersModel> teacherlist = DBHelper.ExecuteList<TeachersModel>("select * from teachers where indentify=1", CommandType.Text, null);
        List<SpareTimeModel> splist = DBHelper.ExecuteList<SpareTimeModel>("select * from sparetime", CommandType.Text, null);
        List<ClassesModel> classlist = DBHelper.ExecuteList<ClassesModel>("select * from classes", CommandType.Text, null);
        // GET: SupervisorPage
        //public ActionResult CheifSupervisor()
        //{
        //    return View();
        //}
        public PartialViewResult Reference()
        {
            return PartialView();
        }
        public ActionResult ReferenceList(string year = "", string month = "", string day = "", int page = 1)
        {
            int i = 1;
            Common com = new Common();
            List<ReferenceModel> referencelist = new List<ReferenceModel>();
            if (year != "" && month != "" && day != "")
            {

                int a = int.Parse(month);
                int thisday = CalendarTools.weekdays(CalendarTools.CaculateWeekDay(int.Parse(year), int.Parse(month), int.Parse(day)));
                int thisweek = CalendarTools.WeekOfYear(int.Parse(year), int.Parse(month), int.Parse(day)) - CalendarTools.WeekOfYear(Common.Year, Common.Month, Common.Day) + 1;
                if (Session["Power"].ToString() == "管理员")
                {
                    List<TeachersModel> telist = DBHelper.ExecuteList<TeachersModel>("select * from teachers where college='" + Session["College"].ToString() + "'", CommandType.Text, null);
                    referencelist = (from t in telist
                                     join c in classlist on t.TeacherName equals c.TeacherName
                                     where t.College == Session["College"].ToString() && c.Week == thisweek && c.Day == thisday
                                     select new ReferenceModel
                                     {
                                         Cid = c.Cid,
                                         Id = i++,
                                         time = CalendarTools.getdata(Common.Year, thisweek, thisday - CalendarTools.weekdays(CalendarTools.CaculateWeekDay(Common.Year, Common.Month, Common.Day)), Common.Month, Common.Day).ToLongDateString() + "" + com.AddSeparator(c.ClassNumber) + "节",
                                         TeacherName = c.TeacherName,
                                         Address = c.Address,
                                         Major = c.Major,
                                         ClassType = c.ClassType,
                                         SupervisorsSum = numbersupervisor(c.Week, c.Day, c.ClassNumber)
                                     }).ToList();
                }
                else
                {
                    referencelist = (from c in classlist
                                     where c.Week == thisweek && c.Day == thisday
                                     select new ReferenceModel
                                     {
                                         Id = i++,
                                         Cid = c.Cid,
                                         time = CalendarTools.getdata(Common.Year, thisweek, thisday - CalendarTools.weekdays(CalendarTools.CaculateWeekDay(Common.Year, Common.Month, Common.Day)), Common.Month, Common.Day).ToLongDateString() + "" + com.AddSeparator(c.ClassNumber) + "节",
                                         TeacherName = c.TeacherName,
                                         Address = c.Address,
                                         Major = c.Major,
                                         ClassType = c.ClassType,
                                         SupervisorsSum = numbersupervisor(c.Week, c.Day, c.ClassNumber)
                                     }).ToList();
                }
            }
            else
            {
                if (Session["Power"].ToString() == "管理员")
                {
                    List<TeachersModel> telist = DBHelper.ExecuteList<TeachersModel>("select * from teachers where college='" + Session["College"].ToString() + "'", CommandType.Text, null);
                    referencelist = (from t in telist
                                     join c in classlist on t.TeacherName equals c.TeacherName
                                     where t.College == Session["College"].ToString()
                                     select new ReferenceModel
                                     {
                                         Id = i++,
                                         Cid = c.Cid,
                                         time = CalendarTools.getdata(Common.Year, c.Week, c.Day - CalendarTools.weekdays(CalendarTools.CaculateWeekDay(Common.Year, Common.Month, Common.Day)), Common.Month, Common.Day).ToLongDateString() + "" + com.AddSeparator(c.ClassNumber) + "节",
                                         TeacherName = c.TeacherName,
                                         Address = c.Address,
                                         Major = c.Major,
                                         ClassType = c.ClassType,
                                         SupervisorsSum = numbersupervisor(c.Week, c.Day, c.ClassNumber)
                                     }).ToList();
                }
                else
                {
                    referencelist = (from c in classlist
                                     select new ReferenceModel
                                     {
                                         Id = i++,
                                         Cid = c.Cid,
                                         time = CalendarTools.getdata(Common.Year, c.Week, c.Day - CalendarTools.weekdays(CalendarTools.CaculateWeekDay(Common.Year, Common.Month, Common.Day)), Common.Month, Common.Day).ToLongDateString() + "" + com.AddSeparator(c.ClassNumber) + "节",
                                         TeacherName = c.TeacherName,
                                         Address = c.Address,
                                         Major = c.Major,
                                         ClassType = c.ClassType,
                                         SupervisorsSum = numbersupervisor(c.Week, c.Day, c.ClassNumber)
                                     }).ToList();
                }
            }
            IPagedList<ReferenceModel> iplist = referencelist.ToPagedList(page, 10);
            return PartialView(iplist);
        }
        public ActionResult ReferenceSure(string cid)
        {
            var classesl = (from c in classlist
                            where c.Cid == cid
                            select new
                            {
                                c.Cid,
                                c.Week,
                                c.Day,
                                c.ClassNumber,
                                c.TeacherName,
                                c.ClassType
                            }).First();
            return RedirectToAction("ArrageAddallselect", new { week = classesl.Week, day = classesl.Day, classnumber = classesl.ClassNumber, teachername = classesl.TeacherName, classtype = classesl.ClassType });
        }
        public ActionResult Supervisor()
        {
            return PartialView();
        }

        /// <summary>
        /// 显示督导信息总览表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public PartialViewResult SupervisorList(int page = 1)
        {
            List<SupervisorViewModel> spvlist = new List<SupervisorViewModel>();
            foreach (TeachersModel teacher in teacherlist)
            {
                SupervisorViewModel m = new SupervisorViewModel();
                m.Tid = teacher.Tid;
                m.TeacherName = teacher.TeacherName;
                m.Phone = teacher.Phone;
                m.Password = teacher.Password;
                m.SpareTime = "";
                var sptlist = (from s in splist
                               where s.Tid == teacher.Tid
                               select s).ToList();
                if (sptlist.Count > 0)
                {
                    List<SpareTimeModel> sptlist1 = new List<SpareTimeModel>();
                    sptlist1 = sptlist.GroupBy(a => a.Week).Select(b => b.First()).ToList();
                    foreach (SpareTimeModel spt in sptlist1)
                    {
                        m.SpareTime = m.SpareTime + " " + spt.Week.ToString();
                    }
                }
                else
                {
                    m.SpareTime = "未填写";
                }
                spvlist.Add(m);
            }

            IPagedList<SupervisorViewModel> Iteachers = spvlist.ToPagedList(page, 10);
            return PartialView(Iteachers);
        }

        /// <summary>
        /// 显示督导老师姓名
        /// </summary>
        /// <param name="p">页码</param>
        /// <returns></returns>
        public string SupervisorName(int p = 0)
        {
            string c = "";
            if (Session["Power"].ToString() == "管理员")
            {
                var te = (from t in teacherlist
                          where t.College == Session["College"].ToString()
                          select new
                          {
                              t.TeacherName,
                              t.Tid
                          }).ToList();
                for (int i = p * 9; i < 9 * p + 9; i++)
                {
                    if (i < te.Count)
                        c += "<li name='del' value='" + te[i].Tid + "'>" + te[i].TeacherName + "<a  href='#tab2' ></a></li>";
                }
            }
            else
            {
                var te = (from t in teacherlist
                          select new
                          {
                              t.TeacherName,
                              t.Tid
                          }).ToList();
                for (int i = p * 9; i < 9 * p + 9; i++)
                {
                    if (i < te.Count)
                        c += "<li name='del' value='" + te[i].Tid + "'>" + te[i].TeacherName + "<a  href='#tab2' ></a></li>";
                }
            }
            return c;
        }

        /// <summary>
        /// 显示手动填补
        /// </summary>
        /// <param name="tid">默认值为测试所用</param>
        /// <returns></returns>
        public string ShowSpareTime(string tid)
        {
            //Encoding e = Encoding.GetEncoding(tid);
            //-----freetime----------
            try
            {
                List<string> sl = new List<string>();
                var weeks = (from s in splist
                            where  s.Tid ==tid.ToString()
                            select s.Week).Distinct();
                Dictionary<int, object> d = new Dictionary<int, object>();
                foreach (var week in weeks)
                {
                    for (int i = 1; i < 8; i++)
                    {
                        
                        //classnol 里的数据有问题
                        string selectsparetime = string.Format("select classnumber from sparetime where tid='{0}' and week='{1}' and day='{2}'", tid, week, i);
                        var classnol = (from sp in splist
                                        where sp.Week == week && sp.Tid == tid.ToString() && sp.Day == i
                                        select sp.ClassNumber).ToList();
                        //List<int> classnol = DBHelper.ExecuteList<int>(selectsparetime, CommandType.Text, null);
                        if (classnol.Count > 0)
                        {
                            int[] stra = classnumberl(classnol).ToArray();
                            string s0 = string.Join(",", stra);
                            string s1 = string.Format("\"{0}\":[{1}]", i, s0);
                            sl.Add(s1);
                        }
                    }
                    string s = string.Join(",", sl);
                    string freetime = string.Format("{{{0}}}", s);
                    d.Add(week, Newtonsoft.Json.JsonConvert.DeserializeObject(freetime));
                }
                return mkjson.show("success", 0, d);
            }
            catch (Exception ex) { return mkjson.show("获取失败！\n" + ex.Message); }

        }
        public ActionResult SaveSpareTime(string tid, string week, string freetime)
        {
            try
            {
                foreach (int w in Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(week))
                {
                    var ft = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, int[]>>(freetime);
                    string deletesql = string.Format("delete from sparetime where tid='{0}' and week='{1}'",tid, w);
                    DBHelper.ExecuteNonQuery(deletesql, CommandType.Text, null);
                    //-----sparetime-----------
                    foreach (var f in ft)
                    {
                        foreach (int c in classesnummerge(f.Value))
                        {
                            //string updatesql = string.Format("update sparetime set classnumber={0} where tid='{1}' and week='{2}' and day='{3}'and classnumber =", c, tid, w, f.Key);
                            //if (DBHelper.ExecuteNonQuery(updatesql, CommandType.Text, null)==0)
                            //{
                            try
                            {
                                string insertsql = string.Format("insert into sparetime (tid,week,day,classnumber,assign) values('{0}','{1}','{2}','{3}','{4}')", tid, w, f.Key, c, 0);
                                DBHelper.ExecuteNonQuery(insertsql, CommandType.Text, null);
                            }
                            catch(Exception e)
                            {
                                //if(e.Message.Contains("重复的主键"))
                                continue;
                            }
                              
                            //}
                        }

                    }
                    //-----sparetime end-----------
                }
                return Json( new { tid=tid});
            }
            catch (Exception ex) { return Json( new { _code = 1,_msg="保存失败" } ); }
        }
        //自动填补空闲时间
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AutoSpare(FormCollection fc)
        {
            var cherkbox = from x in fc.AllKeys
                               //where fc[x] == "on"
                           select x;
            foreach (var cherkname in cherkbox)
            {
                var t = (from te in teacherlist
                         where te.Tid == cherkname
                         select te.TeacherName).First();
                MakeSpareTime.AutoSelectSpareTime(t);
                //string i=list[index].TeacherName;
                //string a;
            }
            return Redirect("/#!/Supervisor/Supervisor");
        }
        public ActionResult ArrageAddallselect(string week, string day, string classnumber, string teachername, string classtype)
        {
            int[] select = new int[] { int.Parse(week), int.Parse(day), int.Parse(classnumber) };
            List<CheckClassModel> checkclasslist = DBHelper.ExecuteList<CheckClassModel>("select * from checkclass", CommandType.Text, null);

            ArrageAddModel arrageadd = new ArrageAddModel();
            if (Session["Power"].ToString() == "管理员")
            {
                List<TeachersModel> telist = DBHelper.ExecuteList<TeachersModel>("select * from teachers where college='" + Session["College"].ToString() + "'", CommandType.Text, null);
                arrageadd.classeslist = (from c in classlist
                                         join t in telist on c.TeacherName equals t.TeacherName
                                         where c.Week == @select[0] && c.Day == @select[1] && c.ClassNumber == @select[2] && c.TeacherName == teachername
                                         && c.ClassType == classtype
                                         select c).ToList();
                arrageadd.FirstSupervisorList = (from s in splist
                                                 join t in teacherlist on s.Tid equals t.Tid
                                                 where t.College == Session["College"].ToString() && s.Week == @select[0] && s.Day == @select[1] && s.ClassNumber == @select[2]
                                                 select new FirstSupervisorModel
                                                 {
                                                     Tid = t.Tid,
                                                     TeacherName = t.TeacherName,
                                                     IsArrage = Trueflase(s.Assign)
                                                 }).ToList();
                arrageadd.SecondSupervisorList = (from ch in checkclasslist
                                                  join t in teacherlist on ch.Tid equals t.Tid
                                                  where t.College == Session["College"].ToString()
                                                  select new SecondSupervisorModel
                                                  {
                                                      Tid = t.Tid,
                                                      TeacherName = t.TeacherName,
                                                      Total = ch.total
                                                  }).ToList();
            }
            else
            {
                arrageadd.classeslist = (from c in classlist
                                         where c.Week == @select[0] && c.Day == @select[1] && c.ClassNumber == @select[2] && c.TeacherName == teachername
                                         && c.ClassType == classtype
                                         select c).ToList();
                arrageadd.FirstSupervisorList = (from s in splist
                                                 join t in teacherlist on s.Tid equals t.Tid
                                                 where s.Week == @select[0] && s.Day == @select[1] && s.ClassNumber == @select[2]
                                                 select new FirstSupervisorModel
                                                 {
                                                     Tid = t.Tid,
                                                     TeacherName = t.TeacherName,
                                                     IsArrage = Trueflase(s.Assign)
                                                 }).ToList();
                arrageadd.SecondSupervisorList = (from ch in checkclasslist
                                                  join t in teacherlist on ch.Tid equals t.Tid
                                                  select new SecondSupervisorModel
                                                  {
                                                      Tid = t.Tid,
                                                      TeacherName = t.TeacherName,
                                                      Total = ch.total
                                                  }).ToList();
            }
            return Json(arrageadd, JsonRequestBehavior.AllowGet);
        }
        private string Trueflase(int i)
        {
            string chiness_tureorflase;
            if (i == 1)
            {
                chiness_tureorflase = "已安排";
                return chiness_tureorflase;
            }
            else
            {
                chiness_tureorflase = "未安排";
                return chiness_tureorflase;
            }

        }

        /// <summary>
        /// 获得督导员数目
        /// </summary>
        /// <param name="week"></param>
        /// <param name="day"></param>
        /// <param name="classnumber"></param>
        /// <returns></returns>
        private int numbersupervisor(int week, int day, int classnumber)
        {
            var count = from sp in splist
                        where sp.Week == week && sp.Day == day && sp.ClassNumber == classnumber
                        group sp.Tid by sp into spl
                        select spl;
            return count.Count();

        }
        private List<int> classesnummerge(int[] clnum)
        {
            List<string> spareclass = new List<string> { "12", "13", "23", "24", "34", "35", "45", "67", "68", "78", "79", "89", "1011", "1112", "1012" };//枚举所有的连续节次
            List<int> list = new List<int>();
            //if (clnum.Length == 1)
            //{
            //    string str = string.Format("{0}", clnum[0]);
            //    foreach(var sp in spareclass)
            //    {
            //        if (sp.Contains(str))
            //        {
            //            list.Add(int.Parse(sp));
            //        }
            //    }
            //}
            //else
            //{
                for (int i = 0; i < clnum.Length; i++)
                {
                    //int j = i + 1;
                    //if(j)
                    for (int j = i + 1; j < clnum.Length; j++)
                    {
                        string str = string.Format("{0}{1}", clnum[i], clnum[j]);
                        foreach (var sp in spareclass)
                        {
                            if (sp==str)
                            {
                                list.Add(int.Parse(sp));
                            }
                        }
                    }
                }
            //}

            return list;
        }
        public ActionResult RmSupervisor(string[] tids)
        {
            try
            {
                foreach (string tid in tids)
                {
                    string updateteachers = string.Format("update teachers set indentify=0 where tid='{0}'", tid);
                    string delectsparetime = string.Format("delete from sparetime where tid='{0}'", tid);
                    DBHelper.ExecuteNonQuery(updateteachers, CommandType.Text, null);
                    DBHelper.ExecuteNonQuery(delectsparetime, CommandType.Text, null);
                }
                return this.Json(new mkjson("删除成功", 0), JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return this.Json(new mkjson("删除失败", 1), JsonRequestBehavior.AllowGet); }
        }
        /// <summary>
        /// 删除教师
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteTeacher()
        {
            try
            {
                Stream s = System.Web.HttpContext.Current.Request.InputStream;
                byte[] b = new byte[s.Length];
                s.Read(b, 0, (int)s.Length);
                string tid = Encoding.UTF8.GetString(b);
                string result = HttpUtility.UrlDecode(tid).Replace("[", "").Replace("]", "");
                string[] ids = result.Split(',');
                string[] idarray = new string[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    idarray[i] = ids[i].Replace('"', ' ').Trim();
                }

                for (int i = 0; i < idarray.Length; i++)
                {
                    string delete_teachers = string.Format("delete from teachers where tid='{0}'", idarray[i]);
                    string delete_classes = string.Format("delete from classes where teachername='{0}'", id2teachername(idarray[i]));
                    string delete_sparetime = string.Format("delete from sparetime where tid='{0}'", idarray[i]);
                    string delete_checkclass = string.Format("delete from checkclass where tid='{0}'", idarray[i]);
                    DBHelper.ExecuteNonQuery(delete_teachers, CommandType.Text, null);
                    DBHelper.ExecuteNonQuery(delete_classes, CommandType.Text, null);
                    DBHelper.ExecuteNonQuery(delete_sparetime, CommandType.Text, null);
                    DBHelper.ExecuteNonQuery(delete_checkclass, CommandType.Text, null);
                }

                return this.Json(new mkjson("删除成功", 0), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return this.Json(new mkjson("删除失败", 1), JsonRequestBehavior.AllowGet);
            }


        }
        //拆分数据库传出来的节次
        private List<int> classnumberl(List<int> l)
        {
            List<int> cl = new List<int>();
            foreach (int classnumber in l)
            {
                int pclassnumber = Convert.ToInt32(classnumber.ToString().Substring(0, classnumber.ToString().Length / 2));
                cl.Add(pclassnumber);
                int nclassnumber = Convert.ToInt32(classnumber.ToString().Substring(classnumber.ToString().Length / 2, classnumber.ToString().Length / 2));
                cl.Add(nclassnumber);
            }
            cl = cl.Distinct().ToList();
            return cl;
        }

        /// <summary>
        /// 将id转换为教师姓名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string id2teachername(string id)
        {

            foreach (TeachersModel teacher in teacherlist)
            {
                if (teacher.Tid.Equals(id))
                {
                    return teacher.TeacherName;
                }
            }
            return "";
        }
    }
}