using System;
using System.Collections.Specialized;

namespace SuperSocket.Http
{
    public class HttpRequest
    {
        public string Method { get; private set; }

        public string Path { get; private set; }

        public string HttpVersion { get; private set; }

        public NameValueCollection Items { get; private set; }

        public string Body { get; set; }
        /// <summary>
        /// 初始化Http请求
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="path">路径</param>
        /// <param name="httpVersion">版本</param>
        /// <param name="items">协议内容</param>
        public HttpRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            Method = method;
            Path = path;
            HttpVersion = httpVersion;
            Items = items;
        }
    }
}
