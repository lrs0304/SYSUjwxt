using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Phone.Tasks;

namespace SYSUjwxt
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 年份
        static readonly string[] Years = { "2008-2009", "2009-2010", "2010-2011", "2011-2012", "2012-2013", "2013-2014","2014-2015","2015-2016"};
        // 学期
        static readonly string[] Semster = { "1", "2", "3" };

        public struct Course
    {
        public String jc;
        public String kcmc;
        public String dd;
        public String zfw;
        public String weekpos;


        public override string ToString()
        {
            return kcmc + " " + dd + " " + zfw + "周" + weekpos+ " " + jc;
        }

        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return this.kcmc.Length;
        }
    }

    public struct Score
    {
        // 课程科目
        public String kcmc;
        // 课程类别
        public String kclb;
        // 学分
        public String xf;
        // 最终成绩
        public String cj;
        // 绩点
        public String jd;
        // 排名
        public String pm;

        public override string ToString()
        {
            string temp = "";
            switch (Convert.ToInt32(kclb))
            { 
                case 10:
                    temp = "公必";
                    break;
                case 11:
                    temp = "专必";
                    break;
                case 21:
                    temp = "专选";
                    break;
                case 30:
                    temp = "公选";
                    break;
                default:
                    temp = "其他";
                    break;

            }
            return kcmc + " " + temp + " " + xf + " " + cj + " " + jd + " " + pm + "\n";
        }
    }



        // 构造函数，初始化，数据绑定
        public MainPage()
        {
            InitializeComponent();
            // 数据绑定，分别绑定课表和成绩的选择列表
            this.courseYearPicker.DataContext = Years;
            this.courseSemPicker.DataContext = Semster;
            this.scoreYearPicker.DataContext = Years;
            this.scoreSemPicker.DataContext = Semster;

            // 登录进度条默认不显示
            progress.Visibility = System.Windows.Visibility.Collapsed;
            courseQueryBtn.IsEnabled = false;
            button2.IsEnabled = false;

            string helpinfo = "    这是一个基于WP7的中山大学教务系统的查询客户端，目前支持课表查询和成绩查询。因为WP7平台不支持同步获取网络数据，所以可能在登录或查询时会有困难，多试几次应该可以解决问题。\n"+
                "    另外提供一个网络版的查询系统，可以直接双击教务系统图片，即可进入。（由Maple提供）\n" +
                "    部分解析代码来自‘大三了朝滨’，非常感谢他所做的工作。\n\n\n"+
                "    点击图片可以访问我的微博，欢迎关注，哈哈";
            help.Text = helpinfo;
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(MainPage_BackKeyPress);
            MessageBoxResult msgRst = MessageBox.Show("本程序是为中山大学学生查询课表、成绩而设计的，假若您非中山大学学生，这个程序对您毫无用处，请退出。", "提示", MessageBoxButton.OK);


        }


        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult msgRst = MessageBox.Show("要退出本程序吗？", "提示", MessageBoxButton.OKCancel);
            if (msgRst == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }

        }

        #region UI交互部分
        // 登录按钮
        private void loginbtn_Click(object sender, RoutedEventArgs e)      
        {
            
            if (textBoxname.Text.Length == 0 || textBoxpsw.Password.Length == 0)
            {
                message.Text = "用户名或密码为空，请输入完整";
                return;
            }
            loginbtn.IsEnabled = false;
            progress.Visibility = System.Windows.Visibility.Visible;
            message.Text = "登录中，这个速度取决于很多因素，例如：你的网速。如果很久没有登录成功，请重启程序试试吧，登录成功后才能查询到数据噢~";
            contentStr = "j_username=" + textBoxname.Text + "&j_password=" + textBoxpsw.Password;
            post("http://uems.sysu.edu.cn/jwxt/j_unieap_security_check.do?");
            
            //login();

        }

        // 课表查询
        private void courseQuery_Click(object sender, RoutedEventArgs e)
        {
            String academicYear = courseYearPicker.SelectedItem as String;
            String semester = courseSemPicker.SelectedItem as String;
            if(courseList != null)
                courseList.Clear();
            String urlStr = "http://uems.sysu.edu.cn/jwxt/sysu/xk/xskbcx/xskbcx.jsp?xnd=" + academicYear + "&xq=" + semester;
            get(urlStr);

            textBlock2.Text = "正在获取课表中,稍等哈，如果很久都没有出来结果，请再点击一下查询，或者重启程序";
            
        }

        // 成绩查询
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            String academicYear = scoreYearPicker.SelectedItem as String;
            String semester = scoreSemPicker.SelectedItem as String;
            if (scoreList != null)
                scoreList.Clear();
            textBlock1.Text = "正在获取成绩中，稍等哈，如果很久都没有出来结果，请再点击一下查询，或者重启程序";
            getScores(academicYear, semester);
        }

        #endregion

        void loginSet()
        {
            message.Text = resultStr;
            if (resultStr.Equals("登录成功"))
                loginbtn.Content = "开始使用";
            loginbtn.IsEnabled = false;
            progress.Visibility = System.Windows.Visibility.Collapsed;
            courseQueryBtn.IsEnabled = true;
            button2.IsEnabled = true;
        }

        void courseSet()
        {
            textBlock2.Text = resultStr;
            courseQueryBtn.IsEnabled = true;
        }

        void scoreSet()
        {
            textBlock1.Text = resultStr;
            button2.IsEnabled = true;
        }

        #region HTTP交互部分

        public String UrlStr;
        private HttpWebRequest httpRequest;
        private HttpWebResponse httpResponse;
        private CookieContainer cc = new CookieContainer();

        private String resultStr;
        String contentStr;

        // 向服务器发送POST请求，用于登录
        private void post(String baseUrl)
        {
            try
            {
                this.UrlStr = baseUrl;
                httpRequest = (HttpWebRequest)WebRequest.Create(UrlStr);
                httpRequest.Headers[HttpRequestHeader.KeepAlive] = "true";

                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.CookieContainer = cc;
                httpRequest.BeginGetRequestStream(new AsyncCallback(RequestCallBack), httpRequest);
            }
            catch (Exception e)
            {
                resultStr = e.Message;
            }

        }

        // post函数的回调函数
        void RequestCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;

                Stream postStream = request.EndGetRequestStream(result);
                byte[] argsBytes = Encoding.UTF8.GetBytes(contentStr);
                // Add the post data to the web request
                postStream.Write(argsBytes, 0, argsBytes.Length);
                postStream.Close();

                request.BeginGetResponse(new AsyncCallback(ResponseCallBack), request);
            }
            catch (WebException e)
            {
                resultStr = e.Message;
            }
        }

        // 回调函数的回调函数
        void ResponseCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                httpResponse = (HttpWebResponse)request.EndGetResponse(result);
                StreamReader readerStream = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                resultStr = readerStream.ReadToEnd();

                readerStream.Close();

                if (resultStr.Contains("unieap.cmpPath = \"/jwxt/index.jsp\""))
                {
                    resultStr = "登录成功";


                }
                else
                {
                    resultStr = "登录失败";

                }

                Dispatcher.BeginInvoke(loginSet);

            }
            catch (WebException e)
            {
                resultStr = e.Message;
            }

        }



        // 向服务器发送GET请求，用于获取网页内容
        private void get(String urlStr)
        {
            resultStr = null;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(urlStr);
                httpRequest.Method = "GET";
                httpRequest.Accept = "text/html";
                httpRequest.CookieContainer = cc;
                httpRequest.BeginGetResponse(new AsyncCallback(GetResponseCallBack), httpRequest);
            }
            catch (Exception e)
            {
                resultStr = e.Message;
            }

        }

        // get函数的回调函数
        void GetResponseCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                httpResponse = (HttpWebResponse)request.EndGetResponse(result);
                StreamReader readerStream = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                resultStr = readerStream.ReadToEnd();
                readerStream.Close();
                if (resultStr.Contains("THE-NODE-OF-SESSION-TIMEOUT"))
                {
                    resultStr = "获取课程失败";
                }
                else 
                {
                    courseList = stringToCourse(ref resultStr);
                    if (courseList.Count > 0)
                    {
                        resultStr = "";
                        for (int i = 0; i < courseList.Count; i++)
                        {
                            resultStr += courseList[i].ToString();
                        }
                    }
                    else
                    {
                        resultStr = "无相关信息";
                    }
                    
                }
                Dispatcher.BeginInvoke(courseSet);

            }
            catch (WebException e)
            {
                resultStr = e.Message;
            }

        }

        // 临时全局变量
        String s;

        private void getScores(String xnd, String xq)
        {
            try
            {
                
                httpRequest = (HttpWebRequest)HttpWebRequest.Create("http://uems.sysu.edu.cn/jwxt/xscjcxAction/xscjcxAction.action?method=getKccjList");
                httpRequest.ContentType = "multipart/form-data";
                httpRequest.Accept = "text/plain";                                        //无设置会出现字符串解析错误
                httpRequest.CookieContainer = cc;

                httpRequest.Method = "POST";

                // need to know the json data.
                s = "{header:{\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}},body:{dataStores:{kccjStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"kccjStore\",pageNumber:1,pageSize:10,recordCount:0,rowSetName:\"pojo_com.neusoft.education.sysu.xscj.xscjcx.model.KccjModel\",order:\"t.xn, t.xq, t.kch, t.bzw\"}},parameters:{\"kccjStore-params\": [{\"name\": \"Filter_t.pylbm_0.14250923241738405\", \"type\": \"String\", \"value\": \"'01'\", \"condition\": \" = \", \"property\": \"t.pylbm\"}, {\"name\": \"Filter_t.xn_0.9157393842453891\", \"type\": \"String\", \"value\": \"'"
                + xnd + "'\", \"condition\": \" = \", \"property\": \"t.xn\"}, {\"name\": \"Filter_t.xq_0.5502242433637169\", \"type\": \"String\", \"value\": \"'"
                + xq + "'\", \"condition\": \" = \", \"property\": \"t.xq\"}, {\"name\": \"xh\", \"type\": \"String\", \"value\": \"'"
                + textBoxname.Text.ToString() + "'\", \"condition\": \" = \", \"property\": \"t.xh\"}], \"args\": [\"student\"]}}}";

                WebHeaderCollection w = new WebHeaderCollection();
                w[HttpRequestHeader.KeepAlive] = "true";
                w["render"] = "unieap";
                w[HttpRequestHeader.ContentLength] = s.Length.ToString();


                httpRequest.Headers = w;

                httpRequest.BeginGetRequestStream(new AsyncCallback(RequestCallBack1), httpRequest);

                
            }
            catch (Exception e)
            {
                resultStr = e.Message;
            }
            
            
        }


        // getScore函数的回调函数
        void RequestCallBack1(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                Stream outStream = request.EndGetRequestStream(result);
                byte[] argsBytes = Encoding.UTF8.GetBytes(s);
                // Add the post data to the web request
                outStream.Write(argsBytes, 0, argsBytes.Length);
                outStream.Close();

                request.BeginGetResponse(new AsyncCallback(ResponseCallBack1), request);
            }
            catch (WebException e)
            {
                resultStr = e.Message;
            }
        }
        // 回调函数的回调函数
        void ResponseCallBack1(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                httpResponse = (HttpWebResponse)request.EndGetResponse(result);
                StreamReader readerStream = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                
                resultStr = readerStream.ReadToEnd();

                readerStream.Close();

                scoreList = stringToScore(ref resultStr);

                if (scoreList.Count > 0)
                {
                    resultStr = "";
                    for (int i = 0; i < scoreList.Count; i++)
                    {
                        resultStr += scoreList[i].ToString();
                    }
                }
                else
                {
                    resultStr = "无相关信息";
                }

                

                Dispatcher.BeginInvoke(scoreSet);

            }
            catch (WebException e)
            {
                resultStr = e.Message;
            }

        }


        #endregion


        #region 字符处理部分

        private List<Course> courseList;

        /// <summary>
        /// 从教务系统上获取到课程表的Html代码后，从中解析出课程信息
        /// </summary>
        /// <param name="content"></param>
        /// <returns>返回值为一个存放解析后得到的课程List</returns>
        private List<Course> stringToCourse(ref String content)
        {
            string pat = "jc=.*\n";
            Regex reg = new Regex(pat, RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(content);
            Course[] courses = new Course[mc.Count];
            for (int i = 0; i < mc.Count; ++i)
            {
                String temp = mc[i].Value.Split('=').ElementAt(1);
                stringFilter(ref temp);
                courses[i].jc = temp;
            }

            pat = "kcmc=.*;";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; ++i)
            {
                String temp = mc[i].Value.Split('=').ElementAt(1);
                stringFilter(ref temp);
                courses[i].kcmc = temp;
            }

            pat = "dd=.*;";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; ++i)
            {
                String temp = mc[i].Value.Split('=').ElementAt(1);
                stringFilter(ref temp);
                courses[i].dd = temp;
            }

            pat = "zfw=.*;";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; ++i)
            {
                String temp = mc[i].Value.Split('=').ElementAt(1);
                stringFilter(ref temp);
                courses[i].zfw = temp;
            }

            pat = "weekpos=.*;";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; ++i)
            {
                String temp = mc[i].Value.Split('=').ElementAt(1);
                stringFilter(ref temp);
                courses[i].weekpos = temp;
            }

            List<Course> coursesList = new List<Course>();
            for (int i = 0; i < courses.Count(); ++i)
            {
                if (!coursesList.Contains(courses[i]))
                    coursesList.Add(courses[i]);
            }

            return coursesList;
        }

        /// <summary>
        /// 字符串过滤，将字符串中含";\n'' "都去除
        /// </summary>
        /// <param name="str"></param>
        private void stringFilter(ref String str)
        {
            //String filterStr = ";\n'' \"\\";
            //str = str.Trim(filterStr.ToCharArray());

            string pat = "[`~!@#$%^&*()+=|{}':;',\\[\\]<>/?~！@#￥%……&*（）——+|{}【】‘；：”“’。，、？\"]";
            //string pat = "[;\n\"\\[\\]/]";
            Regex reg = new Regex(pat, RegexOptions.IgnoreCase);
            str = reg.Replace(str, "");
        }

        private List<Score> scoreList;

        private List<Score> stringToScore(ref String content)
        {
            //课程科目
            String pat = "\"kcmc\":\"[^\"]*\"";
            Regex reg = new Regex(pat, RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(content);
            Score[] scores = new Score[mc.Count];
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].kcmc = temp;
            }

            
            //课程类别
            pat = "\"kclb\":\"[^\"]*\"";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].kclb = temp;
            }

            //学分
            pat = "\"xf\":\"[^\"]*\"";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].xf = temp;
            }
            


            //最终成绩
            pat = "\"zzcj\":\"[^\"]*\"";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].cj = temp;
            }
            //绩点
            pat = "\"jd\":\"[^\"]*\"";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].jd = temp;
            }
            //排名
            pat = "\"jxbpm\":\"[^\"]*\"";
            reg = new Regex(pat, RegexOptions.IgnoreCase);
            mc = reg.Matches(content);
            for (int i = 0; i < mc.Count; i++)
            {
                String temp = mc[i].Value.Split(':').ElementAt(1);
                stringFilter(ref temp);
                scores[i].pm = temp;
            }

            List<Score> scoresList = new List<Score>();
            foreach (var item in scores)
            {
                scoresList.Add(item);
            }
            return scoresList;
        }


        #endregion

        private void image1_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://jwxt.lovemaple.info");
            task.Show();
        }

        private void image2_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://weibo.com/wdxtub");
            task.Show();
        }


        
    }
}