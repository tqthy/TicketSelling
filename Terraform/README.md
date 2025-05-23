# Ticket Selling Platform - Terraform

This Terraform configuration sets up the infrastructure for a ticket selling platform on AWS, including:

- VPC with public and private subnets
- RDS PostgreSQL database
- EC2 instances for Docker Swarm (1 manager + N workers)
- Security groups for network security

## Prerequisites

1. Install [Terraform](https://www.terraform.io/downloads.html) (>= 1.0.0)
2. Install [AWS CLI](https://aws.amazon.com/cli/)
3. Configure AWS credentials:
   ```bash
   aws configure
   ```
4. Create an EC2 key pair in your AWS account

## Getting Started

1. Copy the example variables file:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

2. Edit `terraform.tfvars` with your configuration:
   - Update `ec2_key_name` with your EC2 key pair name
   - Set `private_key_path` to the path of your private key file
   - Optionally, set a database password or leave it empty to auto-generate one

3. Initialize Terraform:
   ```bash
   terraform init
   ```

4. Review the execution plan:
   ```bash
   terraform plan
   ```

5. Apply the configuration:
   ```bash
   terraform apply
   ```

## Outputs

After applying the configuration, Terraform will output:
- VPC ID
- Public and private subnet IDs
- Security group IDs
- RDS endpoint
- Manager node public IP

## Accessing the Infrastructure

- Connect to the manager node:
  ```bash
  ssh -i <path-to-private-key> ubuntu@<manager-public-ip>
  ```

- View Docker Swarm status:
  ```bash
  docker node ls
  ```

## Destroying Resources

To destroy all created resources:

```bash
terraform destroy
```

## Security Notes

- The database password is stored in the Terraform state file. For production, consider using AWS Secrets Manager.
- The EC2 key pair private key should be kept secure and not committed to version control.
- Adjust security group rules to be more restrictive in production environments.
