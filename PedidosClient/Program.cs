using Grpc.Net.Client;
using PedidosService;
using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace PedidosClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
 
            using var channel = GrpcChannel.ForAddress("https://localhost:7012");
            var client = new PedidoManager.PedidoManagerClient(channel);

            Console.WriteLine("Bem-vindo ao Cliente de Pedidos gRPC!");

            while (true)
            {
                Console.WriteLine("\nEscolha uma opção:");
                Console.WriteLine("1. Registrar Novo Pedido");
                Console.WriteLine("2. Acompanhar Status de Pedidos (Streaming Bidirecional)");
                Console.WriteLine("3. Sair");
                Console.Write("Opção: ");
                var opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        await RegistrarNovoPedido(client);
                        break;
                    case "2":
                        await AcompanharPedidos(client);
                        break;
                    case "3":
                        Console.WriteLine("Saindo...");
                        return;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }
            }
        }
       
        static async Task RegistrarNovoPedido(PedidoManager.PedidoManagerClient client)
        {
            Console.WriteLine("\n--- Registrar Novo Pedido ---");
            Console.Write("Nome do Cliente: ");
            var clienteNome = Console.ReadLine();
            Console.Write("Descrição do Produto: ");
            var produtoDescricao = Console.ReadLine();
            Console.Write("Valor do Pedido: ");
            double valor;
            while (!double.TryParse(Console.ReadLine(), out valor))
            {
                Console.WriteLine("Valor inválido. Por favor, digite um número.");
                Console.Write("Valor do Pedido: ");
            }

            var request = new NovoPedidoRequest
            {
                ClienteNome = clienteNome,
                ProdutoDescricao = produtoDescricao,
                Valor = valor
            };

            try
            {
                var response = await client.RegistrarPedidoAsync(request);
                Console.WriteLine($"\nPedido registrado com sucesso!");
                Console.WriteLine($"ID do Pedido: {response.PedidoId}");
                Console.WriteLine($"Status: {response.Status}");
                Console.WriteLine($"Mensagem: {response.Mensagem}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Erro ao registrar pedido: {ex.StatusCode} - {ex.Status.Detail}");
            }
        }

       
        static async Task AcompanharPedidos(PedidoManager.PedidoManagerClient client)
        {
            Console.WriteLine("\n--- Acompanhar Status de Pedidos ---");
            Console.WriteLine("Digite os IDs dos pedidos que deseja acompanhar (um por linha). Digite 'fim' para terminar.");

            using var call = client.AcompanharPedidos();

            var sendTask = Task.Run(async () =>
            {
                string pedidoId;
                while ((pedidoId = Console.ReadLine())?.ToLower() != "fim")
                {
                    if (!string.IsNullOrWhiteSpace(pedidoId))
                    {
                        await call.RequestStream.WriteAsync(new PedidoId { Id = pedidoId });
                        Console.WriteLine($"[CLIENTE] Solicitado acompanhamento do pedido: {pedidoId}");
                    }
                }
                await call.RequestStream.CompleteAsync();
                Console.WriteLine("[CLIENTE] Finalizou envio de IDs de pedidos.");
            });

            var readTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var update in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"\n[ATUALIZAÇÃO DO SERVIDOR] Pedido {update.PedidoId}: Novo Status -> {update.NovoStatus} (em {update.Timestamp})");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Acompanhamento de pedidos encerrado pelo servidor ou cliente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro durante o acompanhamento de pedidos: {ex.Message}");
                }
            });

           
            await Task.WhenAny(sendTask, readTask); 
            Console.WriteLine("Pressione Enter para voltar ao menu principal...");
            Console.ReadLine(); 
        }
    }
}