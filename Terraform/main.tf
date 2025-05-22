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
  url = "https://ifconfig.me/ip"
}

module "vpc" {
  source             = "./modules/vpc"
  name               = var.project_name
  vpc_cidr           = var.vpc_cidr
  public_subnet_cidr = var.public_subnet_cidr
  private_subnet_cidr = var.private_subnet_cidr
  az                 = var.availability_zone
}

module "security_groups" {
  source   = "./modules/security_groups"
  name     = var.project_name
  vpc_id   = module.vpc.vpc_id
  db_port  = 5432
  ssh_cidr = "${chomp(data.http.myip.response_body)}/32"
}

module "rds" {
  source              = "./modules/rds"
  name                = var.project_name
  private_subnet_ids  = [module.vpc.private_subnet_id]
  db_engine           = "postgres"
  db_engine_version   = "15.5"  # Using a stable version
  db_instance_class   = "db.t3.micro"
  db_name             = var.db_name
  db_username         = var.db_username
  db_password         = var.db_password != "" ? var.db_password : random_password.db_password.result
  db_allocated_storage = 20
  db_sg_id            = module.security_groups.rds_sg_id
}

# Generate a random password if one isn't provided
resource "random_password" "db_password" {
  length  = 16
  special = true
  override_special = "_%@"
}

module "ec2" {
  source              = "./modules/ec2"
  ami_id              = data.aws_ami.ubuntu_22_04.id
  instance_type       = "t3.micro"
  subnet_id           = module.vpc.public_subnet_id
  security_group_ids  = [module.security_groups.swarm_sg_id]
  key_name            = var.ec2_key_name
  name                = var.project_name
  worker_count        = var.worker_count
  private_key_path    = var.private_key_path
  
  depends_on = [module.rds]  # Ensure RDS is ready before EC2 instances
}
