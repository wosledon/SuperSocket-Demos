using System;
using Microsoft.Extensions.Hosting;

namespace SuperSocket.Server
{
    internal interface IConfigureContainerAdapter
    {
        /// <summary>
        /// 配置容器
        /// </summary>
        /// <param name="hostContext">主机上下文</param>
        /// <param name="containerBuilder">容器构建器</param>
        void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
    }

    internal class ConfigureContainerAdapter<TContainerBuilder> : IConfigureContainerAdapter
    {
        private Action<HostBuilderContext, TContainerBuilder> _action;
        /// <summary>
        /// 初始化配置容器适配器
        /// </summary>
        /// <param name="action"></param>
        public ConfigureContainerAdapter(Action<HostBuilderContext, TContainerBuilder> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        /// <summary>
        /// 配置容器
        /// </summary>
        /// <param name="hostContext">主机上下文</param>
        /// <param name="containerBuilder">容器构建器</param>
        public void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder)
        {
            _action(hostContext, (TContainerBuilder)containerBuilder);
        }
    }
}