using System;
using System.Buffers;
using System.Collections.Specialized;

namespace SuperSocket.WebSocket
{
    public class HttpHeader
    {
        public string Method { get; private set; }

        public string Path { get; private set; }

        public string HttpVersion { get; private set; }

        public string StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        public NameValueCollection Items { get; private set; }

        private HttpHeader()
        {
            
        }
        /// <summary>
        /// 为请求创建
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="path">路径</param>
        /// <param name="httpVersion">Http版本</param>
        /// <param name="items">协议</param>
        /// <returns></returns>
        public static HttpHeader CreateForRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            return new HttpHeader
            {
                Method = method,
                Path = path,
                HttpVersion = httpVersion,
                Items = items
            };
        }
        /// <summary>
        /// 为响应创建
        /// </summary>
        /// <param name="httpVersion">Http版本</param>
        /// <param name="statusCode">状态码</param>
        /// <param name="statusDescription">状态描述</param>
        /// <param name="items">协议</param>
        /// <returns></returns>
        public static HttpHeader CreateForResponse(string httpVersion, string statusCode, string statusDescription, NameValueCollection items)
        {
            return new HttpHeader
            {
                HttpVersion = httpVersion,
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                Items = items
            };
        }
    }
}
