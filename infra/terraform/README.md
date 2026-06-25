# Staging infrastructure (Azure)

Provisions the private staging environment: Container Registry, Container Apps
(running the API, ingress restricted to your IP), and PostgreSQL Flexible Server
with `pgvector`.

## Prerequisites
- `az login` (Azure CLI authenticated to the target subscription)
- Terraform >= 1.5, Docker

## First deploy

1. **Configure variables**
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   # edit terraform.tfvars: subscription_id, client_ip (curl ifconfig.me),
   # postgres_admin_password, openai_api_key, ingest_api_key
   ```

2. **Provision infra** (runs the API on a placeholder image first)
   ```bash
   terraform init
   terraform apply
   ```

3. **Build and push the API image to the new ACR**
   ```bash
   ACR=$(terraform output -raw acr_name)
   LOGIN_SERVER=$(terraform output -raw acr_login_server)
   az acr login --name "$ACR"
   docker build -t "$LOGIN_SERVER/portfoliorag-api:v1" ..   # build context = repo root
   docker push "$LOGIN_SERVER/portfoliorag-api:v1"
   ```
   > The Dockerfile lives at the repo root; run the build from there (`..` above
   > assumes you're in `infra/terraform`).

4. **Point the Container App at your image**
   ```bash
   terraform apply -var "container_image=$LOGIN_SERVER/portfoliorag-api:v1"
   ```
   On boot the app applies EF migrations (`Database__MigrateOnStartup=true`) and
   enables the `vector` extension.

5. **Ingest, then ask**
   ```bash
   APP=$(terraform output -raw app_url)
   curl -X POST "$APP/ingest" -H "X-Ingest-Key: <your ingest_api_key>"
   curl -X POST "$APP/ask" -H "Content-Type: application/json" \
        -d '{"question":"What is Steve'\''s experience with Kafka?"}'
   ```

## Notes
- **Cost**: burstable Postgres (`B_Standard_B1ms`) + a single small Container App
  replica + Basic ACR. Run `terraform destroy` when not in use to stop charges.
- **Private staging**: app ingress is locked to `client_ip`; Postgres is public
  with firewall rules (Azure services + your IP) and enforced SSL.
- **Production hardening** (later): VNet + private endpoint for Postgres,
  managed identity for ACR pulls, Key Vault for secrets, remote Terraform state.
