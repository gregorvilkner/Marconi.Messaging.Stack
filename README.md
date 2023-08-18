# Marconi.Messaging.Stack
The Marconi Messaging Stack allows accessing of local assets by means of using GraphQL for querying and routing of messages securely through an Azure ServiceBus.

# Creating a Marconi Stack from Scratch

## Azure B2C Active Directory

We use Azure B2C to allow users to securely authenticate. After creation of a new B2C tenant we also organize all resource within this tenant.

## Azure Key Vault

We use Azure Key Vault to store access keys for our messaging service bus.