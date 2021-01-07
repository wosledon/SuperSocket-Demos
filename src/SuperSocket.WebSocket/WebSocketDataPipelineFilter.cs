using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.FramePartReader;

namespace SuperSocket.WebSocket
{
    public class WebSocketDataPipelineFilter : PackagePartsPipelineFilter<WebSocketPackage>
    {
        private HttpHeader _httpHeader;

        /// <summary>
        /// -1: default value
        /// 0: ready to preserve bytes
        /// N: the bytes we preserved
        /// </summary>
        private long _consumed = -1;
        /// <summary>
        /// 初始化WebSocket数据管道筛选器
        /// </summary>
        /// <param name="httpHeader">Http协议头</param>
        public WebSocketDataPipelineFilter(HttpHeader httpHeader)
        {
            _httpHeader = httpHeader;
        }
        /// <summary>
        /// 创建WebSocket包
        /// </summary>
        /// <returns>WebSocket包</returns>
        protected override WebSocketPackage CreatePackage()
        {
            return new WebSocketPackage
            {
                HttpHeader = _httpHeader
            };
        }
        /// <summary>
        /// 筛选器
        /// </summary>
        /// <param name="reader">序列阅读器</param>
        /// <returns>WebSocket包</returns>
        public override WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            WebSocketPackage package = default;
            var consumed = _consumed;

            if (consumed > 0)
            {
                var newReader = new SequenceReader<byte>(reader.Sequence);
                newReader.Advance(consumed);
                package = base.Filter(ref newReader);
                consumed = newReader.Consumed;
            }
            else
            {
                package = base.Filter(ref reader);
                // not final fragment or is the last fragment of multiple fragments message
                if (_consumed == 0)
                {
                    consumed = reader.Consumed;
                    reader.Rewind(consumed);
                }
            }
            
            if (consumed > 0)
            {
                if (_consumed < 0) // cleared
                    reader.Advance(consumed);
                else
                    _consumed = consumed;            
            }

            return package;
        }
        /// <summary>
        /// 获取第一个分件阅读器
        /// </summary>
        /// <returns></returns>
        protected override IPackagePartReader<WebSocketPackage> GetFirstPartReader()
        {
            return PackagePartReader.NewReader;
        }
        /// <summary>
        /// 分件阅读器切换后
        /// </summary>
        /// <param name="currentPartReader">当前分件阅读器</param>
        /// <param name="nextPartReader">下一个分件阅读器</param>
        protected override void OnPartReaderSwitched(IPackagePartReader<WebSocketPackage> currentPartReader, IPackagePartReader<WebSocketPackage> nextPartReader)
        {
            if (currentPartReader is FixPartReader)
            {
                // not final fragment or is the last fragment of multiple fragments message
                // _consumed = 0 means we are ready to preserve the bytes
                if (!CurrentPackage.FIN || CurrentPackage.Head != null)
                    _consumed = 0;
            }
        }
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            _consumed = -1;            
            base.Reset();
        }
    }
}
