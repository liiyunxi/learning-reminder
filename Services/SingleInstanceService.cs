using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 单实例控制：保证只有一个常驻进程；二次启动时把参数转发给已运行实例后退出。
    /// </summary>
    public sealed class SingleInstanceService : IDisposable
    {
        private const int PipeConnectTimeoutMilliseconds = 2000;

        private Mutex? _mutex;
        private CancellationTokenSource? _cancellation;
        private bool _ownsMutex;
        private bool _disposed;

        /// <summary>收到来自其他实例的参数（在线程池线程触发，调用方需自行切回 UI 线程）</summary>
        public event EventHandler<string>? ArgumentsReceived;

        /// <summary>当前进程是否为主实例</summary>
        public bool IsPrimaryInstance { get; private set; }

        /// <summary>
        /// 尝试成为主实例；返回 false 表示已有实例在运行。
        /// </summary>
        public bool TryAcquirePrimary()
        {
            try
            {
                _mutex = new Mutex(initiallyOwned: true, AppConstants.SingleInstanceMutexName, out bool createdNew);
                IsPrimaryInstance = createdNew;
                _ownsMutex = createdNew;
            }
            catch (AbandonedMutexException)
            {
                // 上一个实例异常退出，当前进程接管
                IsPrimaryInstance = true;
                _ownsMutex = true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("创建单实例互斥体失败，按多实例方式继续运行", ex);
                IsPrimaryInstance = true;
                _ownsMutex = false;
            }

            return IsPrimaryInstance;
        }

        /// <summary>主实例开始监听二次启动的参数。</summary>
        public void StartListening()
        {
            if (!IsPrimaryInstance)
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            _ = Task.Run(() => ListenLoopAsync(_cancellation.Token));
        }

        /// <summary>把命令行参数发送给已运行的主实例。</summary>
        public bool SendToPrimary(string arguments)
        {
            try
            {
                using NamedPipeClientStream client = new NamedPipeClientStream(
                    ".",
                    AppConstants.SingleInstancePipeName,
                    PipeDirection.Out);
                client.Connect(PipeConnectTimeoutMilliseconds);
                using StreamWriter writer = new StreamWriter(client, new UTF8Encoding(false));
                writer.Write(arguments);
                writer.Flush();
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("转发启动参数到主实例失败", ex);
                return false;
            }
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using NamedPipeServerStream server = new NamedPipeServerStream(
                        AppConstants.SingleInstancePipeName,
                        PipeDirection.In,
                        maxNumberOfServerInstances: 1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
                    await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                    using StreamReader reader = new StreamReader(server, new UTF8Encoding(false));
                    string payload = await reader.ReadToEndAsync().ConfigureAwait(false);
                    ArgumentsReceived?.Invoke(this, payload);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    FileLogger.Error("单实例管道监听异常", ex);
                    // 短暂延迟后重建管道，避免异常情况下忙等占用 CPU
                    await Task.Delay(1000, token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellation?.Cancel();
            _cancellation?.Dispose();

            try
            {
                // 只有真正持有互斥体的进程才能释放，否则会抛同步异常
                if (_ownsMutex)
                {
                    _mutex?.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error("释放单实例互斥体失败", ex);
            }

            _mutex?.Dispose();
        }
    }
}