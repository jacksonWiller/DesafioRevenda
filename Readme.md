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
```