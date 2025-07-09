using Grpc.Core;
using PedidosService; // Nosso namespace gerado pelo .proto
using System.Collections.Concurrent;

namespace PedidosService.Services
{
    public class PedidoManagerService : PedidoManager.PedidoManagerBase
    {
        private readonly ILogger<PedidoManagerService> _logger;
        private static ConcurrentDictionary<string, PedidoResponse> _pedidos = new ConcurrentDictionary<string, PedidoResponse>();

        public PedidoManagerService(ILogger<PedidoManagerService> logger)
        {
            _logger = logger;
        }

        public override Task<PedidoResponse> RegistrarPedido(NovoPedidoRequest request, ServerCallContext context)
        {
            var pedidoId = Guid.NewGuid().ToString();
            var novoPedido = new PedidoResponse
            {
                PedidoId = pedidoId,
                Status = "Pendente",
                Mensagem = $"Pedido de {request.ClienteNome} para {request.ProdutoDescricao} no valor de R${request.Valor:F2} registrado com sucesso."
            };

            _pedidos[pedidoId] = novoPedido;
            _logger.LogInformation($"Pedido {pedidoId} registrado: {novoPedido.Mensagem}");

            return Task.FromResult(novoPedido);
        }
               
        public override async Task AcompanharPedidos(IAsyncStreamReader<PedidoId> requestStream, IServerStreamWriter<PedidoStatusUpdate> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Iniciando acompanhamento de pedidos...");
                       
            var pedidosParaAcompanhar = new HashSet<string>();
           
            _ = Task.Run(async () =>
            {
                await foreach (var pedidoId in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Cliente solicitou acompanhamento do pedido: {pedidoId.Id}");
                    pedidosParaAcompanhar.Add(pedidoId.Id);
                }
                _logger.LogInformation("Cliente parou de enviar pedidos para acompanhamento.");
            });
                       
            while (!context.CancellationToken.IsCancellationRequested)
            {
                foreach (var pedidoId in pedidosParaAcompanhar)
                {
                    if (_pedidos.TryGetValue(pedidoId, out var pedido))
                    {
                        string novoStatus = "Processando";
                       
                        if (pedido.Status == "Pendente")
                        {
                            novoStatus = "Processando";
                        }
                        else if (pedido.Status == "Processando")
                        {
                            novoStatus = "Enviado";
                        }
                        else if (pedido.Status == "Enviado")
                        {
                            novoStatus = "Concluído";
                        }
                        else
                        {
                            novoStatus = "Concluído";
                        }

                        if (pedido.Status != novoStatus)
                        {
                            pedido.Status = novoStatus;
                            var update = new PedidoStatusUpdate
                            {
                                PedidoId = pedidoId,
                                NovoStatus = novoStatus,
                                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            await responseStream.WriteAsync(update);
                            _logger.LogInformation($"Atualização de status enviada para o pedido {pedidoId}: {novoStatus}");
                        }
                    }
                }
                await Task.Delay(5000);
            }

            _logger.LogInformation("Acompanhamento de pedidos finalizado.");
        }
    }
}