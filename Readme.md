Sistema de Gestão de Revendas e Pedidos

O código que você compartilhou implementa um sistema serverless baseado em AWS Lambda para gerenciar revendas, clientes e pedidos, especialmente voltado para distribuição de bebidas.

## Arquitetura do Sistema

O sistema segue uma arquitetura serverless com as seguintes funções Lambda:

1. **CreateRevenda** - Cria novas revendas no sistema
2. **GetRevendas** - Lista todas as revendas cadastradas
3. **CreatePedido** - Registra pedidos de clientes para as revendas
4. **ProcessarPedidos** - Consolida e envia pedidos para a produtora

Os dados são persistidos no **Amazon DynamoDB** e todas as funções usam a API Gateway para exposição HTTP.

## Modelo de Dados

O sistema possui as seguintes entidades principais:

1. **Revenda** (`Shared.Models.Revenda`)
   - Armazena informações como CNPJ, razão social, contatos, telefones
   - Mantém uma coleção de clientes e seus respectivos endereços de entrega

2. **Cliente** (`Shared.Models.Cliente`) 
   - Pertence a uma revenda e possui dados como CNPJ, razão social, endereço
   - Mantém uma lista de pedidos realizados

3. **PedidoCliente** (`Shared.Models.PedidoCliente`)
   - Contém itens de pedido, data, status e observações
   - Status pode ser: Novo, Erro ou Enviado

## Fluxo de Funcionamento

### 1. Criação de Revendas (`CreateRevenda/Function.cs`)

```
Cliente HTTP → API Gateway → Lambda → DynamoDB
```

- Recebe dados JSON com informações da revenda
- Valida dados (CNPJ, email, etc.) usando `RevendaValidator`
- Salva a revenda no DynamoDB

### 2. Listagem de Revendas (`GetRevendas/Function.cs`)

```
Cliente HTTP → API Gateway → Lambda → DynamoDB → Resultado JSON
```

- Recupera todas as revendas do DynamoDB
- Retorna a lista em formato JSON

### 3. Criação de Pedidos (`CreatePedido/Function.cs`)

```
Cliente HTTP → API Gateway → Lambda → Buscar Revenda → Buscar Cliente → Criar Pedido → DynamoDB
```

- Recebe dados do pedido (revendaId, clienteId, itens)
- Localiza a revenda e o cliente correspondentes
- Cria um novo pedido com status "Novo"
- Adiciona o pedido à lista de pedidos do cliente
- Atualiza a revenda no DynamoDB

### 4. Processamento de Pedidos (`ProcessarPedidos/Function.cs`)

```
Trigger → Lambda → Buscar Revendas → Agrupar Pedidos → Enviar para Produtora → Atualizar Status → DynamoDB
```

- Para cada revenda, busca pedidos com status "Novo" ou "Erro"
- Agrupa itens semelhantes de múltiplos pedidos
- Verifica quantidade mínima para processamento (1000 unidades)
- Simula envio para a Produtora com `EnviarPedidoParaProdutoraMock` 
- Atualiza status dos pedidos para "Enviado" ou "Erro"
- Salva as alterações no DynamoDB

## Características Técnicas

1. **Validação de Dados**: 
   - Validação de CNPJ, email, telefone
   - Verificação de campos obrigatórios

2. **Mapeamento de Dados**:
   - Classe `RevendaMapper` converte entre objetos C# e formato DynamoDB

3. **Simulação de Integração Externa**:
   - `EnviarPedidoParaProdutoraMock`: simula com sucesso/falha aleatória
   - `EnviarPedidoParaProdutoraTrue`: prepara para integração real

4. **Tratamento de Erros**:
   - Retorno apropriado de códigos HTTP e mensagens de erro
   - Logs detalhados de exceções com contexto

## Exemplo de Fluxo de Operação

1. **Criar Revenda**: POST para `/revendas` com dados da empresa
2. **Listar Revendas**: GET em `/revendas` para verificar a criação
3. **Criar Pedido**: POST para `/pedidos` com dados do pedido
4. **Processar Pedidos**: POST para `/processar` para consolidar e enviar pedidos

O código está estruturado de forma modular, separando responsabilidades entre modelos, acesso a dados e lógica de negócios. A arquitetura serverless permite escalabilidade e gerenciamento eficiente de recursos.

Collecting workspace information# Configuração do Insomnia para testar sua API

Para testar sua API usando o Insomnia, organize as requisições da seguinte forma:

## 1. GET - Listar Revendas

- **Método:** GET
- **URL:** https://code.execute-api.us-east-1.amazonaws.com/revendas
- **Headers:** 
  - Content-Type: application/json

## 2. POST - Criar Revenda

- **Método:** POST
- **URL:** https://code.execute-api.us-east-1.amazonaws.com/revendas
- **Headers:** 
  - Content-Type: application/json
- **Body (JSON):**

```json
{
  "cnpj": "12345678901234",
  "razaoSocial": "Empresa Exemplo Ltda",
  "nomeFantasia": "Exemplo Comercial",
  "email": "contato@exemplo.com.br",
  "telefones": [
    {
      "numero": "11999998888"
    }
  ],
  "contatos": [
    {
      "nome": "José Silva",
      "principal": true
    }
  ],
  "enderecosEntrega": [
    {
      "logradouro": "Avenida Paulista",
      "numero": "1000",
      "complemento": "Sala 123",
      "bairro": "Bela Vista",
      "cidade": "São Paulo",
      "estado": "SP",
      "cep": "01310100"
    }
  ]
}
```

## 3. POST - Criar Pedido

- **Método:** POST
- **URL:** https://code.execute-api.us-east-1.amazonaws.com/pedidos
- **Headers:** 
  - Content-Type: application/json
- **Body (JSON):**

```json
{
  "revendaId": "62b8f422-637f-41bc-a603-36152e50f8af",
  "clienteId": "f0070935-b05a-4fbd-aeac-8e87a6db9196",
  "itens": [
    {
      "produto": "Cerveja Artesanal IPA",
      "quantidade": 500,
      "precoUnitario": 18.50,
      "valorTotal": 555.00
    },
    {
      "produto": "Cerveja Artesanal Pilsen",
      "quantidade": 40,
      "precoUnitario": 12.75,
      "valorTotal": 510.00
    },
    {
      "produto": "Cerveja Artesanal Stout",
      "quantidade": 15,
      "precoUnitario": 22.90,
      "valorTotal": 343.50
    },
    {
      "produto": "Cerveja Artesanal Weiss",
      "quantidade": 20,
      "precoUnitario": 16.80,
      "valorTotal": 336.00
    }
  ],
  "observacoes": "Manter refrigerado. Entregar somente no período da manhã."
}

# Tutorial para Instalação e Execução do Sistema de Gestão de Revendas e Pedidos

Este tutorial vai mostrar como instalar, configurar e executar o sistema de Gestão de Revendas e Pedidos, uma aplicação serverless baseada em AWS Lambda.

## Requisitos do Sistema

### Ferramentas e Softwares

1. **.NET 8.0 SDK** - Framework para desenvolvimento da aplicação
2. **AWS CLI** - Interface de linha de comando para AWS
3. **AWS SAM CLI** - Para desenvolvimento e teste local de aplicações serverless
4. **Visual Studio 2022** ou **VS Code com extensões C#**
5. **Docker** - Para emular o ambiente Lambda localmente
6. **Conta AWS** com permissões para:
   - AWS Lambda
   - Amazon DynamoDB
   - API Gateway

## 1. Instalação das Ferramentas

### 1.1 .NET 8.0 SDK

Baixe e instale o .NET 8.0 SDK em [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

Verifique a instalação:

```bash
dotnet --version
```

### 1.2 AWS CLI

Baixe e instale a AWS CLI:
- Windows: [Instalador MSI](https://awscli.amazonaws.com/AWSCLIV2.msi)
- Linux/macOS: 
  ```bash
  curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
  unzip awscliv2.zip
  sudo ./aws/install
  ```

Configure suas credenciais AWS:

```bash
aws configure
```

### 1.3 AWS SAM CLI

Instale o AWS SAM CLI:
- Windows: [Instalador MSI](https://github.com/aws/aws-sam-cli/releases/latest/download/AWS_SAM_CLI_64_PY3.msi)
- Linux/macOS:
  ```bash
  brew tap aws/tap
  brew install aws-sam-cli
  ```

Verifique a instalação:

```bash
sam --version
```

### 1.4 Docker

Instale o Docker Desktop em [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)

## 2. Configuração do Projeto

### 2.1 Clone o Repositório

```bash
git clone <URL-DO-REPOSITORIO>
cd DesafioRevenda
```

### 2.2 Restaure as Dependências

```bash
dotnet restore DesafioRevenda.sln
```

### 2.3 Configure o DynamoDB

Crie uma tabela no DynamoDB com o nome `Revendas` (ou configure o ambiente local):

```bash
aws dynamodb create-table \
  --table-name Revendas \
  --attribute-definitions AttributeName=id,AttributeType=S \
  --key-schema AttributeName=id,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST
```

Alternativamente, você pode usar o DynamoDB local para desenvolvimento:

```bash
docker run -p 8000:8000 amazon/dynamodb-local
```

## 3. Executando a Aplicação Localmente

### 3.1 Compilar o Projeto

```bash
dotnet build DesafioRevenda.sln
```

### 3.2 Executar com SAM Local

O SAM CLI permite testar as funções Lambda localmente:

```bash
sam local start-api
```

Isso iniciará uma API local que simula a API Gateway e executa suas funções Lambda localmente.

### 3.3 Testando as Funções Individualmente

Você também pode invocar funções específicas:

#### Testar a função CreateRevenda:

```bash
sam local invoke CreateRevenda --event events/create-revenda.json
```

#### Testar a função GetRevendas:

```bash
sam local invoke GetRevendas
```

#### Testar a função CreatePedido:

```bash
sam local invoke CreatePedido --event events/create-pedido.json
```

#### Testar a função ProcessarPedidos:

```bash
sam local invoke ProcessarPedidos --event events/processar-pedidos.json
```

## 4. Implantando na AWS

### 4.1 Empacotar a Aplicação

```bash
sam package --output-template-file packaged.yaml --s3-bucket SEU-BUCKET-S3
```

### 4.2 Implantar a Aplicação

```bash
sam deploy --template-file packaged.yaml --stack-name desafio-revenda --capabilities CAPABILITY_IAM
```

Ou simplesmente use o comando guiado:

```bash
sam deploy --guided
```

## 5. Testando a API Implantada

Depois que a aplicação estiver implantada, você receberá URLs para as suas APIs. Teste-as usando o Insomnia, Postman ou curl.

### 5.1 Listar Revendas

```bash
curl -X GET https://sua-api-url.amazonaws.com/revendas
```

### 5.2 Criar uma Revenda

```bash
curl -X POST https://sua-api-url.amazonaws.com/revendas \
  -H "Content-Type: application/json" \
  -d '{
  "cnpj": "12345678901234",
  "razaoSocial": "Empresa Exemplo Ltda",
  "nomeFantasia": "Exemplo Comercial",
  "email": "contato@exemplo.com.br",
  "telefones": [{"numero": "11999998888"}],
  "contatos": [{"nome": "José Silva", "principal": true}],
  "enderecosEntrega": [{
    "logradouro": "Avenida Paulista",
    "numero": "1000",
    "complemento": "Sala 123",
    "bairro": "Bela Vista",
    "cidade": "São Paulo",
    "estado": "SP",
    "cep": "01310100"
  }]
}'
```

### 5.3 Criar um Pedido

Primeiro, obtenha IDs válidos para uma revenda e um cliente:

```bash
curl -X POST https://sua-api-url.amazonaws.com/pedidos \
  -H "Content-Type: application/json" \
  -d '{
  "revendaId": "62b8f422-637f-41bc-a603-36152e50f8af",
  "clienteId": "f0070935-b05a-4fbd-aeac-8e87a6db9196",
  "itens": [
    {
      "produto": "Cerveja Artesanal IPA",
      "quantidade": 500,
      "precoUnitario": 18.50
    },
    {
      "produto": "Cerveja Artesanal Pilsen",
      "quantidade": 40,
      "precoUnitario": 12.75
    }
  ],
  "observacoes": "Manter refrigerado"
}'
```

### 5.4 Processar Pedidos

```bash
curl -X POST https://sua-api-url.amazonaws.com/processar
```

## 6. Monitoramento e Logs

### 6.1 Visualizar Logs das Funções Lambda

```bash
aws logs get-log-events --log-group-name /aws/lambda/desafio-revenda-CreateRevenda --log-stream-name <stream-id>
```

### 6.2 Verificar Dados no DynamoDB

```bash
aws dynamodb scan --table-name Revendas
```

## Solução de Problemas

1. **Erro de permissão**: Verifique se seu usuário AWS tem permissões suficientes para criar/gerenciar os recursos
2. **Falha na implantação**: Verifique os logs do CloudFormation para detalhes
3. **Erro 400/500 nas APIs**: Examine os logs do Lambda para identificar exceções
4. **Execução local falha**: Certifique-se de que o Docker está em execução

## Recursos Adicionais

- [Documentação AWS Lambda](https://docs.aws.amazon.com/lambda/)
- [Documentação DynamoDB](https://docs.aws.amazon.com/dynamodb/)
- [Documentação AWS SAM](https://docs.aws.amazon.com/serverless-application-model/)
- [C# no Lambda](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)

Este tutorial fornece os passos básicos para configurar, executar e implantar o sistema de Gestão de Revendas e Pedidos. Para obter mais detalhes sobre funcionalidades específicas ou configurações avançadas, consulte a documentação do projeto.