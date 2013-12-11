using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace SYSUjwxt
{
    public static class MethodCall
    {
        static MethodCall()
        {
            OnComplate += new ComplateCallBack((obj) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _callBack.Invoke(obj);
                });
            });
        }
        private static event ComplateCallBack OnComplate;
        private delegate void ComplateCallBack(object result);

        private static Action<object> _callBack;
        public static void Invoke(Func<object> action, Action<object> callback)
        {
            _callBack = callback;
            ThreadStart t = new ThreadStart(() =>
            {
                OnComplate(action.Invoke());
            });
            new Thread(t).Start();
        }
    }
}

/*
 
 调用的方法* 
 MethodCall.Invoke(
() =>
{
    
        * 实际上这段代码是在新线程里执行的，比如在Click事件里直接这样写，也不会阻塞线程
        

    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.baidu.com/");
    string errText = null;
    // 同步调用GetResponse方法
    HttpWebResponse response = request.GetResponse(ref errText);

    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
    {
        // 返回http请求的结果
        return sr.ReadToEnd();
    }
},
(obj) =>
{
    
        * 这里通过MethodCall在内部执行完上面的方法后调用的，obj参数为上面方法的返回值
        

    // 那么这里就可以获得百度首页的html了
    // 同样如果上面返回的是一个List<string>或者其他的对象
    // 也可以(List<string>)obj或者(xxx)obj来转换，返回值是你可以决定的
    // 当然这也可以改写为一个泛型版，类型转换也不用了
    string html = obj.ToString();
    MessageBox.Show(html);
});
 
 */