using System;
using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;


namespace SuperSocket.Command
{
    public abstract class JsonCommand<TJsonObject> : JsonCommand<IAppSession, TJsonObject>
    {

    }

    public abstract class JsonCommand<TAppSession, TJsonObject> : ICommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public JsonSerializerOptions JsonSerializerOptions { get; }
        /// <summary>
        /// 初始化json命令
        /// </summary>
        public JsonCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">数据包</param>
        public virtual void Execute(TAppSession session, IStringPackage package)
        {
            var content = package.Body;            
            ExecuteJson(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }
        /// <summary>
        /// 执行json
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="jsonObject">json对象</param>
        protected abstract void ExecuteJson(TAppSession session, TJsonObject jsonObject);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="content">json字符串</param>
        /// <returns>json对象</returns>
        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content);
        }
    }

    public abstract class JsonAsyncCommand<TJsonObject> : JsonAsyncCommand<IAppSession, TJsonObject>
    {

    }

    public abstract class JsonAsyncCommand<TAppSession, TJsonObject> : IAsyncCommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public JsonSerializerOptions JsonSerializerOptions { get; }
        /// <summary>
        /// 初始化Json异步命令
        /// </summary>
        public JsonAsyncCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        /// <summary>
        /// 异步执行
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        public virtual async ValueTask ExecuteAsync(TAppSession session, IStringPackage package)
        {
            var content = package.Body;
            await ExecuteJsonAsync(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }
        /// <summary>
        /// 反序列化Json字符串
        /// </summary>
        /// <param name="content">Json字符串</param>
        /// <returns></returns>
        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content, JsonSerializerOptions);
        }
        /// <summary>
        /// 执行Json异步命令
        /// </summary>
        /// <param name="session"></param>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        protected abstract ValueTask ExecuteJsonAsync(TAppSession session, TJsonObject jsonObject);
    }
}
