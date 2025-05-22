output "vpc_id"           { value = module.vpc.vpc_id }
output "public_subnet_id" { value = module.vpc.public_subnet_id }
output "private_subnet_id" { value = module.vpc.private_subnet_id }
output "swarm_sg_id"      { value = module.security_groups.swarm_sg_id }
output "rds_endpoint"     { value = module.rds.db_endpoint }
output "manager_public_ip" { value = module.ec2.manager_public_ip }