provider "aws" {
  region = var.aws_region
  default_tags {
    tags = var.tags
  }
}

data "aws_ami" "ubuntu_22_04" {
  most_recent = true
  owners      = ["099720109477"] # Canonical

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }


  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

# Get the current public IP for security group rules
data "http" "myip" {
  url = "https://api.ipify.org?format=text"
}

# Get the current public IPv4 address for security group rules
data "http" "myipv4" {
  url = "https://ipv4.icanhazip.com"
}

module "vpc" {
  source              = "./modules/vpc"
  name                = var.project_name
  vpc_cidr            = var.vpc_cidr
  public_subnet_cidr = [
    cidrsubnet(var.vpc_cidr, 8, 1),  # 10.10.1.0/24
    cidrsubnet(var.vpc_cidr, 8, 2)   # 10.10.2.0/24
  ]
  private_subnet_cidr = [
    cidrsubnet(var.vpc_cidr, 8, 3),   # 10.10.3.0/24
    cidrsubnet(var.vpc_cidr, 8, 4)    # 10.10.4.0/24
  ]
  availability_zones = [
    "${var.aws_region}a",
    "${var.aws_region}b"
  ]
  
}

module "security_groups" {
  source   = "./modules/security_groups"
  name     = var.project_name
  vpc_id   = module.vpc.vpc_id
  db_port  = 5432
  ssh_cidr = "${chomp(data.http.myipv4.response_body)}/32"
}


# Only create RDS if enabled
variable "create_rds" {
  description = "Whether to create RDS instance"
  type        = bool
  default     = true
}

module "rds" {
  count               = var.create_rds ? 1 : 0
  source              = "./modules/rds"
  name                = var.project_name
  private_subnet_ids  = module.vpc.private_subnet_ids
  db_engine           = "postgres"
  db_engine_version   = "17.5"
  db_instance_class   = "db.m5.large"
  db_name             = var.db_name
  db_username         = var.db_username
  db_password         = var.db_password != "" ? var.db_password : random_password.db_password.result
  db_allocated_storage = 20
  db_sg_id            = module.security_groups.rds_sg_id
  tags                = var.tags
}

# Generate a random password if one isn't provided
resource "random_password" "db_password" {
  length  = 16
  special = true
  override_special = "_%@"
}

# Create the SSH key pair for EC2 instances
resource "tls_private_key" "ssh_key" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

resource "aws_key_pair" "ticket_selling_key" {
  key_name   = var.ec2_key_name
  public_key = tls_private_key.ssh_key.public_key_openssh
}

# Save private key to file
resource "local_file" "private_key" {
  content         = tls_private_key.ssh_key.private_key_pem
  filename        = var.private_key_path
  file_permission = "0400"
}

module "ec2" {
  source              = "./modules/ec2"
  ami_id              = data.aws_ami.ubuntu_22_04.id
  instance_type       = "t3.micro"
  subnet_id           = module.vpc.public_subnet_id
  security_group_ids  = [module.security_groups.swarm_sg_id]
  key_name            = aws_key_pair.ticket_selling_key.key_name
  name                = var.project_name
  worker_count        = var.worker_count
  private_key_path    = local_file.private_key.filename
  
  depends_on = [aws_key_pair.ticket_selling_key, local_file.private_key]  # Ensure dependencies are ready before EC2 instances
}
