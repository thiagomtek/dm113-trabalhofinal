# Sistema de Emissão e Acompanhamento de Pedidos com gRPC

Este projeto demonstra um sistema de comunicação distribuída utilizando gRPC para gerenciar a emissão e o acompanhamento de pedidos. Ele é composto por uma aplicação de servidor gRPC e uma aplicação cliente console.

## Funcionalidades Implementadas

* **Registro de Pedidos (Operação Unária):** Permite ao cliente registrar um novo pedido enviando seus detalhes ao servidor.
* **Acompanhamento de Status de Pedidos (Streaming Bidirecional):** O cliente pode solicitar o acompanhamento de um ou mais pedidos pelo ID, e o servidor enviará atualizações em tempo real sobre o status desses pedidos.

## Tecnologias

* **gRPC:** Framework de RPC de alta performance e código aberto.
* **.NET 8.0:** Plataforma de desenvolvimento.
* **Protobuf (Protocol Buffers):** Linguagem de serialização de dados utilizada pelo gRPC.

## Estrutura do Projeto

* **`PedidosService/`**: Projeto do servidor gRPC.
    * `Protos/pedidos.proto`: Definição do contrato de serviço gRPC (mensagens e serviços).
    * `Services/PedidoManagerService.cs`: Implementação da lógica de negócios do servidor para o gerenciamento de pedidos.
    * `Program.cs`: Configuração do servidor gRPC.
* **`PedidosClient/`**: Projeto do cliente console gRPC.
    * `pedidos.proto`: Cópia do arquivo .proto para geração de código do cliente.
    * `Program.cs`: Lógica do cliente para interagir com o servidor gRPC.

## Como Executar

1.  **Pré-requisitos:**
    * Visual Studio 2022 (ou versão mais recente) com a workload "Desenvolvimento ASP.NET e web" instalada.
    * SDK do .NET 8.0 (ou versão mais recente).

2.  **Clonar o Repositório:**
    ```bash
    git clone https://github.com/thiagomtek/dm113-trabalhofinal.git
    cd [pedidosService]
    ```

3.  **Abrir a Solução no Visual Studio:**
    Abra o arquivo `PedidoService.sln` com o Visual Studio.

4.  **Configurar Projetos de Inicialização:**
    * No Gerenciador de Soluções, clique com o botão direito na solução `PedidoService`.
    * Selecione "Definir Projeto de Inicialização...".
    * Escolha "Vários projetos de inicialização".
    * Defina a "Ação" para "Iniciar" tanto para `PedidosService` quanto para `PedidosClient`.
    * Clique em "OK".

5.  **Executar:**
    Pressione `F5` ou clique no botão "Iniciar" no Visual Studio. Duas janelas de console serão abertas: uma para o servidor e outra para o cliente.

6.  **Interagir com o Cliente:**
    No console do cliente, siga as opções apresentadas no menu para:
    * **Registrar Novo Pedido:** Digite `1`, forneça os detalhes do pedido e anote o `ID do Pedido` gerado.
    * **Acompanhar Status de Pedidos:** Digite `2`, e então digite os `ID(s) do(s) Pedido(s)` que você deseja acompanhar (um por linha). Digite `fim` para parar de enviar IDs. O cliente começará a exibir as atualizações de status em tempo real. Pressione Enter para voltar ao menu principal.
