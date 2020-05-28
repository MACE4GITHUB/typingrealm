using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.TcpServer
{
    public sealed class TcpServer : AsyncManagedDisposable
    {
        private readonly ILogger<TcpServer> _logger;
        private readonly IConnectionHandler _connectionHandler;
        private readonly IProtobufConnectionFactory _protobufConnectionFactory;
        private readonly TcpListener _tcpListener;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<Task> _connectionProcessors = new List<Task>();
        private Task? _listeningProcess;
        private bool _isStopped;

        public TcpServer(
            int port,
            ILogger<TcpServer> logger,
            IConnectionHandler connectionHandler,
            IProtobufConnectionFactory protobufConnectionFactory)
        {
            _logger = logger;
            _connectionHandler = connectionHandler;
            _protobufConnectionFactory = protobufConnectionFactory;
            _tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_listeningProcess != null)
                throw new InvalidOperationException("Server has already started.");

            _tcpListener.Start();
            _listeningProcess = ProcessAsync();
        }

        public ValueTask StopAsync()
        {
            ThrowIfDisposed();

            if (_listeningProcess == null)
                throw new InvalidOperationException("Server has not started yet.");

            return DisposeAsync();
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            if (_listeningProcess == null)
            {
                _cts.Dispose();
                return;
            }

            // Indicates that no more connections should be accepted.
            // Used solely in ProcessAsync() method to escape while loop.
            _isStopped = true;

            _cts.Cancel();

            await _listeningProcess
                .HandleCancellationAsync(exception =>
                {
                    _logger.LogDebug(exception, "Cancellation request received for listening process. Stopped listening for incoming connections.");
                })
                .ConfigureAwait(false);

            _listeningProcess = null;

            // Each connection processor is responsible for handling it's own exceptions.
            // They should all gracefully complete.
            await Task.WhenAll(_connectionProcessors).ConfigureAwait(false);

            _tcpListener.Stop();
            _connectionProcessors.Clear();
            _cts.Dispose();
        }

        /// <summary>
        /// This task never ends until it is stopped using StopAsync method.
        /// </summary>
        private async Task ProcessAsync()
        {
            while (!_isStopped)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync()
                    .WithCancellationAsync(_cts.Token)
                    .ConfigureAwait(false);

                _cts.Token.ThrowIfCancellationRequested();

                var task = Handle(tcpClient);
                _connectionProcessors.Add(task);
                _connectionProcessors.RemoveAll(t => t.IsCompleted);
            }
        }

        private async Task Handle(TcpClient tcpClient)
        {
            var connectionDetails = string.Empty;

            try
            {
                connectionDetails = tcpClient.Client.RemoteEndPoint.ToString() ?? "No details";

                using var stream = tcpClient.GetStream();
                using var sendLock = new SemaphoreSlimLock();
                using var receiveLock = new SemaphoreSlimLock();
                var connection = _protobufConnectionFactory.CreateProtobufConnection(stream)
                    .WithLocking(sendLock, receiveLock);

                await _connectionHandler
                    .HandleAsync(connection, _cts.Token)
                    .HandleCancellationAsync(exception =>
                    {
                        _logger.LogDebug($"Cancellation request received for client: {connectionDetails}");
                    })
                    .ConfigureAwait(false);
            }
#pragma warning disable CA1031 // This method should not throw ANY exceptions, it is a top-level handler.
            catch (Exception exception)
#pragma warning restore CA1031
            {
                _logger.LogError(exception, $"Error happened while handling TCP connection, connection details: {connectionDetails}");
            }
            finally
            {
                tcpClient.Dispose();
            }
        }
    }
}
