# PdfMerge Function

Azure Functions .NET 8 (modelo isolado) que recebe uma lista de hashes/nomes de
arquivo, busca cada PDF correspondente em um container do Azure Blob Storage
e devolve um único PDF com todos os arquivos unificados, na ordem informada.

## Arquitetura

```
src/
├── PdfMerge.Functions/        # Trigger HTTP, Program.cs, host.json (camada de apresentação)
├── PdfMerge.Application/      # Caso de uso (PdfMergeService), DTOs, validação
├── PdfMerge.Domain/           # Exceções de domínio, sem dependências externas
└── PdfMerge.Infrastructure/   # Implementação do acesso ao Blob Storage
```

## Pré-requisitos

- .NET 8 SDK
- Azure Functions Core Tools v4
- Uma Storage Account (ou o [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) para rodar localmente)
- No Azurite você deve ter um container criado e no arquivo local.settings.json passar o nome deste container para a var InputContainerName

## Configuração

Edite `src/PdfMerge.Functions/local.settings.json`:

```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "BlobStorage:ConnectionString": "<sua connection string>",
    "BlobStorage:InputContainerName": "pdfs-input"
  }
}
```

Em produção, configure `BlobStorage:ConnectionString` como uma referência de
Key Vault na App Setting, em vez de valor em texto puro.

## Executando localmente

```bash
dotnet restore
dotnet build
cd src/PdfMerge.Functions
func start
```

## Endpoint

**POST** `/api/merge`

Corpo da requisição:

```json
{
  "fileHashes": [
    "3f9c2a1b8e7d4f6a9c0b1d2e3f4a5b6c.pdf",
    "8a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d.pdf"
  ],
  "outputFileName": "contrato-completo.pdf"
}
```

- `fileHashes`: obrigatório, lista ordenada dos nomes de blob a buscar e unificar
  (máximo de 50 itens por requisição, configurável em `MergePdfsRequestValidator`).
- `outputFileName`: opcional; se omitido, um nome é gerado automaticamente.

Resposta: binário `application/pdf` (o próprio arquivo unificado), com o
cabeçalho `Content-Disposition` sugerindo o nome do arquivo para download.

### Respostas de erro

| Status | Código           | Quando acontece                                      |
|--------|------------------|-------------------------------------------------------|
| 400    | `invalid_request`| Lista vazia, nula, com itens inválidos ou acima do limite |
| 404    | `file_not_found` | Algum hash informado não existe no container de origem |
| 422    | `invalid_pdf`    | O blob existe mas não é um PDF válido/legível          |
| 500    | `internal_error` | Erro inesperado                                        |

## Testes

```bash
dotnet test
```

Os testes cobrem o `PdfMergeService` com um `IBlobStorageService` mockado
(via Moq), gerando PDFs falsos em memória com PdfSharp para validar a
contagem de páginas e os cenários de erro.

## Decisões de design

- **PdfSharp** (MIT) para leitura/escrita/merge de PDFs — evita dependências
  com licenciamento AGPL/comercial.
- **Azure.Storage.Blobs** para acesso ao blob, escondido atrás de
  `IBlobStorageService` — troque a implementação sem tocar na Application.
- **Middleware de exceção único** (`ExceptionHandlingMiddleware`) mapeia
  exceções de domínio para status HTTP, mantendo as Functions limpas.
- **Streams em memória**: para arquivos muito grandes ou muitos arquivos por
  requisição, considere trocar `MemoryStream` por arquivos temporários em
  disco (`/tmp` no Linux Consumption) para não estourar o limite de memória
  da instância.

## Provisionamento de Infra

Para provisionar a infra necessária para o deploy usando o workflow do Git Actions você precisará de uma conta no Azure, Um Resource group criado e configurado, um Storage Account criado, configurado e com ao menos um container, também será necessario criar um Azure Function App dentro desde resource group.

Para facilitar o provisionamento do primeiro deploy, criei um .sh que está dentro da pasta infra que tem a função de provisionar a configuração de acesso para deploy do Git -> Azure, configurando credencials e permissões.

## Como rodar o .sh ? 

# Permissão para executar
````bash
chmod +x AzureFunctionDeployConfig.sh
````

# Executando
```bash
./AzureFunctionDeployConfig.sh \ nome-do-app \ resource-group \ function-app \ "codigo-gerado-ao-consultar-seu-OIDC-no-settings-do-repo"
```
