# API para Gestão de Estoque e Produtos - Sistema de Instrumentos Musicais

Solução backend completa para controle de estoque e gerenciamento de catálogo de produtos desenvolvida em .NET 8.0, utilizando arquitetura de microsserviços com comunicação assíncrona via Apache Kafka.

## Índice

- [Descrição](#descrição)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Executando o Projeto](#executando-o-projeto)
- [Fluxo Completo de Uso](#fluxo-completo-de-uso)
- [Endpoints da API](#endpoints-da-api)
- [Autenticação](#autenticação)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)

## Descrição

Sistema de gestão de estoque e produtos para instrumentos musicais, permitindo:

- **Cadastro e gerenciamento de usuários** (Administradores e Vendedores)
- **Autenticação via JWT**
- **CRUD completo de produtos**
- **Controle de estoque** com registro de notas fiscais
- **Emissão de pedidos** com validação de estoque em tempo real via Kafka

A solução utiliza arquitetura de microsserviços, garantindo escalabilidade e independência entre os serviços.

##  Arquitetura

O projeto é composto por 3 microsserviços independentes:

```
┌─────────────────┐
│  Identity API   │  → Gerenciamento de usuários e autenticação
│   Porta: 5070   │
└─────────────────┘

┌─────────────────┐
│  Catalog API    │  → Gestão de produtos e estoque
│   Porta: 5066   │  → Consome eventos de pedidos via Kafka
└─────────────────┘

┌─────────────────┐
│  Orders API     │  → Gestão de pedidos
│   Porta: 5045   │  → Valida estoque via Kafka antes de criar pedido
└─────────────────┘

┌─────────────────┐
│   Kafka + ZK    │  → Mensageria para comunicação assíncrona
│   Porta: 9092   │
└─────────────────┘

┌─────────────────┐
│  PostgreSQL     │  → Banco de dados (3 instâncias separadas)
│   Portas:       │     - Identity: 5435
│   5433-5435     │     - Catalog: 5433
└─────────────────┘     - Orders: 5434
```

### Fluxo de Validação de Estoque

Quando um pedido é criado:

1. **Orders API** publica evento `StockValidationRequestEvent` via Kafka
2. **Catalog API** consome o evento e valida o estoque
3. **Catalog API** publica resposta `StockValidationResponseEvent` via Kafka
4. **Orders API** consome a resposta e:
   - Se válido: cria o pedido e publica `OrderCreatedEvent`
   - Se inválido: retorna erro ao cliente
5. **Catalog API** consome `OrderCreatedEvent` e realiza a baixa no estoque

## Tecnologias

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **Apache Kafka** - Mensageria (microsserviços)
- **JWT** - Autenticação
- **Docker & Docker Compose** - Containerização
- **xUnit** - Testes unitários
- **Moq** - Mocks para testes

## Pré-requisitos

- [Docker](https://www.docker.com/get-started) (versão 20.10 ou superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versão 2.0 ou superior)

##  Executando o Projeto

### 1. Clone o repositório

```bash
git clone <url-do-repositorio>
cd AlerquimTeste
```

### 2. Execute o Docker Compose

O comando abaixo irá subir todos os serviços necessários:

```bash
docker-compose up --build
```

Este comando irá:
- Criar e iniciar os containers do PostgreSQL (3 instâncias)
- Iniciar o Zookeeper e Kafka
- Compilar e iniciar os 3 microsserviços
- Executar as migrations automaticamente

### 3. Aguarde a inicialização

Aguarde alguns minutos para que todos os serviços iniciem completamente. Você pode verificar os logs:

```bash
docker-compose logs -f
```

### 4. Acesse a documentação Swagger

Após a inicialização, acesse:

- **Identity API**: http://localhost:5070/swagger
- **Catalog API**: http://localhost:5066/swagger
- **Orders API**: http://localhost:5045/swagger

## Fluxo Completo de Uso

Siga os passos abaixo para percorrer o fluxo completo da aplicação:

### Passo 1: Cadastrar Usuário Administrador

**Endpoint**: `POST http://localhost:5070/api/User/register`

**Request**:
```json
{
  "nome": "Admin Usuario",
  "email": "admin@example.com",
  "password": "senha123",
  "userRole": "Admin"
}
```

**Response**:
```json
{
  "id": "guid-aqui",
  "nome": "Admin Usuario",
  "email": "admin@example.com",
  "role": "Admin"
}
```

### Passo 2: Cadastrar Usuário Vendedor

**Endpoint**: `POST http://localhost:5070/api/User/register`

**Request**:
```json
{
  "nome": "Vendedor Teste",
  "email": "vendedor@example.com",
  "password": "senha123",
  "userRole": "Seller"
}
```

### Passo 3: Realizar Login

**Endpoint**: `POST http://localhost:5070/api/Auth/login`

**Request**:
```json
{
  "email": "admin@example.com",
  "password": "senha123"
}
```

**Response**:
```json
{
  "token": {
    "userId": "guid-aqui",
    "nome": "Admin Usuario",
    "role": "Admin",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**️ Importante**: Copie o `token` retornado. Você precisará usá-lo em todas as requisições autenticadas.

### Passo 4: Cadastrar Produto (Admin)

**Endpoint**: `POST http://localhost:5066/Product/CreateProduct`

**Headers**:
```
Authorization: Bearer {token-do-passo-3}
```

**Request**:
```json
{
  "nome": "Guitarra Elétrica",
  "descricao": "Guitarra elétrica profissional Les Paul",
  "valor": 2500.00
}
```

**Response**:
```json
{
  "id": "guid-do-produto",
  "nome": "Guitarra Elétrica",
  "descricao": "Guitarra elétrica profissional Les Paul",
  "valor": 2500.00,
  "estoqueQTD": 0
}
```

**Cadastre mais produtos** (exemplo):
```json
{
  "nome": "Bateria Acústica",
  "descricao": "Bateria acústica completa 5 peças",
  "valor": 3500.00
}
```

### Passo 5: Adicionar Estoque (Admin)

**Endpoint**: `POST http://localhost:5066/Product/AddStock?productId={guid-do-produto}&quantity=10&invoiceNumber=NF001/2024`

**Headers**:
```
Authorization: Bearer {token-do-passo-3}
```

**Response**: `200 OK`

**Adicione estoque para os produtos cadastrados.**

### Passo 6: Listar Produtos (Vendedor)

**Endpoint**: `GET http://localhost:5066/Product/GetAll`

**Headers**:
```
Authorization: Bearer {token-vendedor}
```

**Response**:
```json
[
  {
    "id": "guid-aqui",
    "nome": "Guitarra Elétrica",
    "descricao": "Guitarra elétrica profissional Les Paul",
    "valor": 2500.00,
    "estoqueQTD": 10
  },
  {
    "id": "guid-aqui-2",
    "nome": "Bateria Acústica",
    "descricao": "Bateria acústica completa 5 peças",
    "valor": 3500.00,
    "estoqueQTD": 10
  }
]
```

### Passo 7: Criar Pedido (Vendedor)

**Endpoint**: `POST http://localhost:5045/Order/CreateOrder`

**Headers**:
```
Authorization: Bearer {token-vendedor}
```

**Request**:
```json
{
  "documentoUsuario": "12345678900",
  "vendedor": "Vendedor Teste",
  "items": [
    {
      "productId": "guid-guitarra",
      "qtd": 2
    },
    {
      "productId": "guid-bateria",
      "qtd": 1
    }
  ]
}
```

**Response** (sucesso):
```json
{
  "id": "guid-do-pedido",
  "documentoUsuario": "12345678900",
  "vendedor": "Vendedor Teste",
  "items": [
    {
      "productId": "guid-guitarra",
      "qtd": 2
    },
    {
      "productId": "guid-bateria",
      "qtd": 1
    }
  ]
}
```

**O sistema automaticamente:**
1.  Valida o estoque via Kafka
2.  Cria o pedido
3.  Realiza a baixa no estoque

**Response** (estoque insuficiente):
```json
{
  "error": "Validação de estoque falhou: Estoque insuficiente para um ou mais produtos"
}
```

## 🔌 Endpoints da API

### Identity API (Porta 5070)

#### Autenticação
- `POST /api/Auth/login` - Login (retorna JWT token)

#### Usuários
- `POST /api/User/register` - Cadastrar usuário

### Catalog API (Porta 5066)

**Requer autenticação JWT**

#### Produtos
- `POST /Product/CreateProduct` - Criar produto (Admin)
- `GET /Product/GetAll` - Listar todos os produtos
- `GET /Product/{id}` - Buscar produto por ID
- `PUT /Product/{id}` - Atualizar produto (Admin)
- `DELETE /Product/{id}` - Deletar produto (Admin)

#### Estoque
- `POST /Product/AddStock?productId={guid}&quantity={int}&invoiceNumber={string}` - Adicionar estoque (Admin)

### Orders API (Porta 5045)

**Requer autenticação JWT**

#### Pedidos
- `POST /Order/CreateOrder` - Criar pedido (Valida estoque via Kafka)
- `GET /Order/GetAllOrders` - Listar todos os pedidos (Admin)
- `GET /Order/{id}` - Buscar pedido por ID (Admin)

##  Autenticação

Todas as APIs (exceto registro e login) requerem autenticação via JWT.

### Como obter o token:

1. Faça login em `/api/Auth/login`
2. Copie o token retornado
3. Use no header de todas as requisições:

```
Authorization: Bearer {seu-token-aqui}
```

### Regras de Autorização:

- **Admin**: Acesso completo (CRUD produtos, adicionar estoque, ver todos os pedidos)
- **Seller**: Pode visualizar produtos e criar pedidos

### Validações de Cadastro:

- Email deve ser único
- Senha deve ter **mínimo de 6 caracteres**
- Role deve ser `Admin` ou `Seller`

## Testes

Execute os testes unitários:

```bash
# Testes do Identity
dotnet test Identity.Tests/Identity.Tests.csproj

# Testes do Catalog
dotnet test Catalog.Tests/Catalog.Tests.csproj

# Testes do Orders
dotnet test Orders.Tests/Orders.Tests.csproj

# Todos os testes
dotnet test
```

### Cobertura de Testes

- Testes de domínio (validações de negócio)
- Testes de aplicação (lógica de serviços)
- Testes de integração entre componentes

## Estrutura do Projeto

```
AlerquimTeste/
├── Identity.API/              # API de autenticação e usuários
│   ├── Controllers/
│   ├── Dockerfile
│   └── Program.cs
│
├── Identity.Application/       # Lógica de aplicação (Identity)
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/
│
├── Identity.Domain/           # Domínio (Identity)
│   └── Users/
│
├── Identity.Infrastructure/   # Infraestrutura (Identity)
│   ├── Data/
│   └── Migrations/
│
├── Identity.Tests/            # Testes (Identity)
│
├── Catalog.API/               # API de produtos
│   ├── Controllers/
│   ├── Dockerfile
│   └── Program.cs
│
├── Catalog.Application/       # Lógica de aplicação (Catalog)
│   ├── Services/
│   ├── DTOs/
│   │   └── Events/
│   └── Interfaces/
│
├── Catalog.Domain/            # Domínio (Catalog)
│   └── Products/
│
├── Catalog.Infrastructure/    # Infraestrutura (Catalog)
│   ├── Data/
│   ├── Messaging/             # Kafka consumers/publishers
│   └── Migrations/
│
├── Catalog.Tests/             # Testes (Catalog)
│
├── Orders.API/                # API de pedidos
│   ├── Controllers/
│   ├── Dockerfile
│   └── Program.cs
│
├── Orders.Application/        # Lógica de aplicação (Orders)
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/
│
├── Orders.Domain/             # Domínio (Orders)
│   ├── Events/
│   └── Orders/
│
├── Orders.Infrastructure/     # Infraestrutura (Orders)
│   ├── Data/
│   ├── Messaging/             # Kafka consumers/publishers
│   └── Migrations/
│
├── Orders.Tests/              # Testes (Orders)
│
└── docker-compose.yml         # Orquestração de todos os serviços
```

##  Funcionalidades Implementadas

###  Histórias de Usuário (H1-H5)

- **H1 - Cadastro de Usuários**:  Implementado
  - Seleção de tipo (Admin/Seller)
  - Validação de email único
  - Validação de senha mínima (6 caracteres)

- **H2 - Login**:  Implementado
  - Login via email
  - Retorno de token JWT

- **H3 - Gerenciamento de Produtos**: Implementado
  - CRUD completo
  - Validações de negócio

- **H4 - Controle de Estoque**: Implementado
  - Adição de estoque com nota fiscal
  - Rastreabilidade de movimentações

- **H5 - Emissão de Pedidos**: Implementado
  - Validação de estoque via Kafka **ANTES** de criar pedido
  - Baixa automática no estoque após criação
  - Mensagens de erro claras

###  Diferenciais Implementados

- **Microsserviços** com comunicação via **Kafka**
- **Validação assíncrona de estoque** antes de criar pedido
- **Arquitetura limpa** (DDD) com separação de responsabilidades
- **Testes unitários** com boa cobertura

## Troubleshooting

### Erro: "Timeout ao aguardar validação de estoque"

**Causa**: O Kafka pode não estar totalmente iniciado.

**Solução**: Aguarde alguns minutos e tente novamente. Verifique os logs do Kafka:
```bash
docker-compose logs kafka
```

### Erro de conexão com banco de dados

**Causa**: PostgreSQL ainda não iniciou completamente.

**Solução**: Aguarde o healthcheck dos containers. Verifique:
```bash
docker-compose ps
```

### Token JWT inválido

**Causa**: Token expirado ou formato incorreto.

**Solução**: 
1. Faça login novamente
2. Certifique-se de usar o formato: `Bearer {token}` no header

## Notas Importantes

- As migrations são executadas automaticamente na inicialização
- O Kafka cria tópicos automaticamente quando necessário
- Cada serviço possui seu próprio banco de dados PostgreSQL
- Os logs de cada serviço podem ser visualizados no Docker Compose

##  Desenvolvimento

### Tecnologias e Padrões

- **Domain-Driven Design (DDD)**
- **Clean Architecture**
- **Repository Pattern**
- **CQRS (simplificado)**
- **Event-Driven Architecture** (via Kafka)

### Convenções de Código

- Nomes em português para domínio de negócio
- Código em inglês para infraestrutura técnica
- Exceções de domínio para regras de negócio
- DTOs para comunicação entre camadas

---

