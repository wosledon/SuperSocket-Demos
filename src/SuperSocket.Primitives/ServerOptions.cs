using System.Collections.Generic;
using System.Text;
using SuperSocket.Channel;

namespace SuperSocket
{
public class ServerOptions : ChannelOptions
{
/// <summary>
/// 名字
/// </summary>
public string Name { get; set; }
/// <summary>
/// 监听的端口列表
/// </summary>
public List<ListenOptions> Listeners { get; set; }
/// <summary>
/// 默认文本编码
/// </summary>
public Encoding DefaultTextEncoding { get; set; }
/// <summary>
/// 清除闲置的Session时间间隔
/// </summary>
public int ClearIdleSessionInterval { get; set; } = 120;
/// <summary>
/// 限制Session超时
/// </summary>
public int IdleSessionTimeOut { get; set; } = 300;
}
}