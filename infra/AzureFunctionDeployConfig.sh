#!/bin/bash

set -euo pipefail

if [ "$#" -ne 4 ]; then
    echo "Uso:"
    echo "  $0 <app-name> <resource-group> <function-app> <github-subject>"
    echo
    echo "Exemplo:"
    echo "  $0 gh-actions azure-functions AzurePdfHelper \"codigo-gerado-ao-consultar-seu-OIDC-no-settings-do-repo\""
    exit 1
fi

APP_NAME="$1"
RESOURCE_GROUP="$2"
FUNCTION_APP="$3"
GH_SUBJECT="$4"

echo "Obtendo informações da assinatura..."

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
TENANT_ID=$(az account show --query tenantId -o tsv)

echo "Criando App Registration..."

az ad app create --display-name "$APP_NAME" > /dev/null

APP_ID=$(az ad app list \
    --display-name "$APP_NAME" \
    --query "[0].appId" \
    -o tsv)

echo "Criando Service Principal..."

az ad sp create --id "$APP_ID" > /dev/null

echo "Criando credencial federada..."

az ad app federated-credential create \
    --id "$APP_ID" \
    --parameters "{
        \"name\": \"github-main-branch\",
        \"issuer\": \"https://token.actions.githubusercontent.com\",
        \"subject\": \"$GH_SUBJECT\",
        \"audiences\": [
            \"api://AzureADTokenExchange\"
        ]
    }"

echo "Concedendo permissão Contributor..."

az role assignment create \
    --assignee "$APP_ID" \
    --role Contributor \
    --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"

echo
echo "========================================="
echo "Configuração concluída!"
echo
echo "Adicione estes valores nos Secrets do GitHub:"
echo
echo "AZURE_CLIENT_ID=$APP_ID"
echo "AZURE_TENANT_ID=$TENANT_ID"
echo "AZURE_SUBSCRIPTION_ID=$SUBSCRIPTION_ID"
echo "========================================="