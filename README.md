# Agenda Interativa | Interactive Salon Scheduler

**A salon appointment, service, inventory, and payment management app.**

**Aplicativo para gestão de agenda, serviços, estoque e pagamentos de salão.**

![Next.js](https://img.shields.io/badge/Next.js-App_Router-black?logo=next.js)
![TypeScript](https://img.shields.io/badge/TypeScript-typed-blue?logo=typescript)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

---

## English

### About

Interactive Salon Scheduler is a full-stack portfolio project for managing a hair salon. It brings appointments, the service catalog, customer records, product stock, checkout, receivables, and revenue summaries into one responsive application.

### Features

- Schedule appointments for a customer with multiple services and optional products; totals are calculated automatically.
- Browse appointments by month and day, update pending appointments, and mark services as completed.
- Receive dashboard reminders 20 minutes before an appointment and one day before an installment is due.
- Maintain customer contact details and product inventory.
- Close an appointment with a payment method or create up to five receivables using equal monthly installments.
- Review customer financial statements, daily revenue, and date-range reports.
- Manage the salon service catalog, categories, variations, hair lengths, and prices.
- Choose Pink, Gold Gradient, or grayscale Dark visual themes.

### Technology

- **Frontend:** Next.js App Router, React, TypeScript, and Tailwind CSS.
- **Backend:** .NET 8 ASP.NET Core Web API.
- **Data:** PostgreSQL 16, Entity Framework Core, and Npgsql.
- **Local development:** Docker Compose; the Windows helper script `inicia.bat` builds and starts the services.

The backend is organized as an ASP.NET Core API project with controllers, domain entities and request DTOs, and an EF Core data context. The frontend and backend communicate through the real HTTP API.

### Repository layout

```text
backend/                 .NET 8 Web API and Dockerfile
frontend/                Next.js app, components, and Dockerfile
docker-compose.yml        Local PostgreSQL, API, and web services
inicia.bat                Windows Docker build/start helper
Salao.sln                 .NET solution
NuGet.Config              NuGet package source configuration
README.md                 Project overview (English and Portuguese)
```

### Run locally

Requirements: Docker Desktop with Docker Compose.

```bash
docker compose up --build -d
```

Open the app at [http://localhost:3000](http://localhost:3000). The API is available at [http://localhost:5000](http://localhost:5000), and Swagger UI at [http://localhost:5000/swagger](http://localhost:5000/swagger). PostgreSQL data is persisted in the `salao_data` Docker volume.

On Windows, `inicia.bat` can be used to build and start the stack.

### Deploy the frontend to Vercel

This repository contains both a Next.js frontend and a .NET API. Deploy the frontend to Vercel and host the .NET API and PostgreSQL database on a service that supports containers and managed PostgreSQL.

1. Import this repository in Vercel and set **Root Directory** to `frontend`.
2. Set the Vercel environment variable `NEXT_PUBLIC_API_URL` to the public API URL ending in `/api`, for example `https://api.example.com/api`.
3. Deploy `backend/Salao.Api` using its Dockerfile to a .NET/container host, and provision a persistent PostgreSQL database.
4. Set the API environment variables `ConnectionStrings__Salon`, `Cors__AllowedOrigins`, `Admin__AccessCode`, and `Admin__ChangeCode` on the API host. `Cors__AllowedOrigins` accepts semicolon-separated frontend origins, such as `https://your-app.vercel.app;https://www.example.com`.
5. Set the admin access/change codes to private values before publishing a live salon deployment. The checked-in defaults are intended only for local development.

The API creates its initial database schema and seeds the salon's starter service catalog on first startup. Keep the database on persistent storage.

### Configuration

| Variable | Used by | Purpose |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | Frontend | Base URL for the API; defaults to `http://localhost:5000/api`. |
| `ConnectionStrings__Salon` | Backend | PostgreSQL connection string. |
| `Cors__AllowedOrigins` | Backend | Semicolon-separated allowed frontend origins. |
| `Admin__AccessCode` | Backend | Administrator access code. |
| `Admin__ChangeCode` | Backend | Code required to confirm catalog and inventory changes. |

---

## Português

### Sobre

Agenda Interativa é um projeto full-stack de portfólio para gestão de salão de beleza. Reúne agendamentos, catálogo de serviços, cadastro de clientes, estoque de produtos, fechamento de atendimento, contas a receber e demonstrativos financeiros em uma aplicação responsiva.

### Funcionalidades

- Agendamento de clientes com vários serviços e produtos opcionais, com soma automática dos valores.
- Navegação dos agendamentos por mês e dia, edição de agendamentos pendentes e conclusão do atendimento.
- Avisos no dashboard 20 minutos antes do horário marcado e um dia antes do vencimento de parcelas.
- Cadastro de clientes com telefone e controle de estoque de produtos.
- Fechamento com forma de pagamento ou criação de até cinco parcelas mensais iguais.
- Demonstrativo financeiro por cliente, ganhos do dia e relatórios por período.
- Manutenção do catálogo de serviços, categorias, variações, comprimentos de cabelo e preços.
- Temas visuais Rosa Pink, Ouro Gradiente e Dark em escala de cinza.

### Tecnologias

- **Frontend:** Next.js App Router, React, TypeScript e Tailwind CSS.
- **Backend:** .NET 8 ASP.NET Core Web API.
- **Dados:** PostgreSQL 16, Entity Framework Core e Npgsql.
- **Desenvolvimento local:** Docker Compose; o script `inicia.bat` compila e inicia os serviços no Windows.

O backend está organizado como um projeto ASP.NET Core API, com controllers, entidades de domínio e DTOs de entrada, além do contexto de dados do EF Core. O frontend consome a API HTTP real.

### Estrutura do repositório

```text
backend/                 API .NET 8 e Dockerfile
frontend/                Aplicação Next.js, componentes e Dockerfile
docker-compose.yml        PostgreSQL, API e aplicação web locais
inicia.bat                Script Windows para compilar/iniciar Docker
Salao.sln                 Solução .NET
NuGet.Config              Configuração das fontes NuGet
README.md                 Apresentação do projeto em inglês e português
```

### Executar localmente

Requisitos: Docker Desktop com Docker Compose.

```bash
docker compose up --build -d
```

Acesse a aplicação em [http://localhost:3000](http://localhost:3000). A API fica em [http://localhost:5000](http://localhost:5000), com Swagger em [http://localhost:5000/swagger](http://localhost:5000/swagger). Os dados do PostgreSQL permanecem no volume Docker `salao_data`.

No Windows, também é possível executar `inicia.bat` para compilar e iniciar os serviços.

### Publicar o frontend na Vercel

Este repositório contém o frontend Next.js e a API .NET. Publique o frontend na Vercel e hospede a API .NET e o PostgreSQL em um serviço que aceite containers e banco gerenciado.

1. Importe este repositório na Vercel e defina **Root Directory** como `frontend`.
2. Configure a variável `NEXT_PUBLIC_API_URL` na Vercel com o endereço público da API terminado em `/api`, por exemplo `https://api.exemplo.com/api`.
3. Publique `backend/Salao.Api` usando seu Dockerfile em um host compatível com .NET/containers e crie um banco PostgreSQL persistente.
4. Configure na API `ConnectionStrings__Salon`, `Cors__AllowedOrigins`, `Admin__AccessCode` e `Admin__ChangeCode`. `Cors__AllowedOrigins` aceita origens separadas por ponto e vírgula, como `https://seu-app.vercel.app;https://www.exemplo.com`.
5. Defina códigos administrativos privados antes de usar o sistema com dados reais. Os valores padrão versionados servem apenas para desenvolvimento local.

Na primeira inicialização, a API cria o esquema inicial e cadastra o catálogo base de serviços. Mantenha o banco em armazenamento persistente.

### Configuração

| Variável | Usada por | Finalidade |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | Frontend | Endereço base da API; padrão `http://localhost:5000/api`. |
| `ConnectionStrings__Salon` | Backend | String de conexão do PostgreSQL. |
| `Cors__AllowedOrigins` | Backend | Origens permitidas do frontend, separadas por ponto e vírgula. |
| `Admin__AccessCode` | Backend | Código de acesso administrativo. |
| `Admin__ChangeCode` | Backend | Código para confirmar alterações no catálogo e estoque. |
