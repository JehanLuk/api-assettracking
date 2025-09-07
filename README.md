# **AssetTrackingAPI**

API simples para registro e rastreamento de ativos em empresas, permitindo CRUD (criação, leitura, atualização e deleção de ativos), incluido controle por status da situação do ativo na empresa (`Active`,`InUse`,`Maintenance`, `Retired`,`Sold`).

---

## Tecnologias do Projeto

* Linguagem e Framework: .NET 9 (ASP.NET Core)
* Banco de Dados: PostgreSQL
* ORM: Entity Framework Core 9
* Containerização: Docker & Docker Compose
* Cloud: Oracle Cloud Service

## Execução do docker localmente (ainda em andamento)

### 1. Clone o repositório

```
git clone https://github.com/JehanLuk/api-assettracking.git
cd api-assettracking
```

### 2. Crie o arquivo de ambiente: Crie um arquivo .env na raiz do projeto. Ele é essencial para configurar as credenciais do banco de dados local. Use o seguinte template:

```
# Credenciais para o banco de dados PostgreSQL da sua máquina
POSTGRES_USER=root
POSTGRES_PASSWORD=root
POSTGRES_DB=TodoAppDb
```

### 3. Execute o Docker Compose: Use o arquivo docker-compose.dev.yml, que subirá tanto a API quanto um contêiner com o banco de dados PostgreSQL.

```
# nota: o nome "docker compose" faz parte da versão mais recente do pacote. 
docker compose -f docker-compose.dev.yml up --build
```