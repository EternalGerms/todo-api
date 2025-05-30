# API de Lista de Tarefas (To-Do List API)

Projeto desenvolvido conforme o desafio do roadmap.sh: [Todo List API](https://roadmap.sh/projects/todo-list-api)

## Descrição

Esta é uma API RESTful para gerenciamento de listas de tarefas (to-do list), com autenticação de usuários, CRUD de tarefas, paginação, filtros, segurança, rate limiting, refresh token e mensagens de erro padronizadas.

## Funcionalidades

- Cadastro de usuário
- Login com geração de JWT e refresh token
- CRUD de tarefas (apenas o dono pode editar/deletar)
- Paginação, filtro e ordenação de tarefas
- Validação de dados
- Mensagens de erro padronizadas
- Rate limiting (20 requisições por minuto por IP)
- Refresh token para renovação do JWT
- Respostas e rotas em português brasileiro

## Tecnologias
- ASP.NET Core 9
- Entity Framework Core (InMemory)
- JWT (JSON Web Token)
- Rate Limiting Middleware

## Como rodar o projeto

1. Clone o repositório
2. Execute `dotnet restore` para instalar as dependências
3. Execute `dotnet run` para iniciar a API
4. Acesse os endpoints conforme a documentação abaixo

## Endpoints principais

### Cadastro de usuário
```
POST /registrar
{
  "nome": "João Silva",
  "email": "joao@exemplo.com",
  "senha": "minhasenha"
}
```
**Resposta:**
```
{
  "token": "...",
  "refreshToken": "..."
}
```

### Login
```
POST /login
{
  "email": "joao@exemplo.com",
  "senha": "minhasenha"
}
```
**Resposta:**
```
{
  "token": "...",
  "refreshToken": "..."
}
```

### Refresh Token
```
POST /refresh
{
  "refreshToken": "..."
}
```
**Resposta:**
```
{
  "token": "...",
  "refreshToken": "..."
}
```

### Criar tarefa
```
POST /api/tarefas
Headers: Authorization: Bearer {token}
{
  "titulo": "Comprar pão",
  "descricao": "Pão francês e integral",
  "status": "Pendente"
}
```
**Resposta:**
```
{
  "id": 1,
  "titulo": "Comprar pão",
  "descricao": "Pão francês e integral",
  "status": "Pendente"
}
```

### Listar tarefas (com paginação, filtro e ordenação)
```
GET /api/tarefas?pagina=1&limite=10&status=Pendente&ordenarPor=titulo&ordem=asc
Headers: Authorization: Bearer {token}
```
**Resposta:**
```
{
  "dados": [ ... ],
  "pagina": 1,
  "limite": 10,
  "total": 2
}
```

### Atualizar tarefa
```
PUT /api/tarefas/{id}
Headers: Authorization: Bearer {token}
{
  "titulo": "Comprar pão e leite",
  "descricao": "Pão francês, integral e leite",
  "status": "Concluida"
}
```

### Deletar tarefa
```
DELETE /api/tarefas/{id}
Headers: Authorization: Bearer {token}
```

## Mensagens de erro padronizadas
- 401 Não autorizado: `{ "mensagem": "Não autorizado: Token não fornecido ou inválido." }`
- 403 Proibido: `{ "mensagem": "Proibido: Você não tem permissão para acessar/alterar esta tarefa." }`
- 404 Não encontrada: `{ "mensagem": "Não encontrada: Tarefa não localizada." }`
- 429 Rate limit: `{ "mensagem": "Too Many Requests: Limite de requisições atingido. Tente novamente em instantes." }`

## Observações
- O banco de dados é em memória (InMemory), ideal para testes e desenvolvimento.
- O refresh token é armazenado em memória para fins didáticos. Para produção, recomenda-se persistir em banco de dados.
- Todas as rotas e mensagens estão em português brasileiro.

## Referência do desafio
[https://roadmap.sh/projects/todo-list-api](https://roadmap.sh/projects/todo-list-api)
