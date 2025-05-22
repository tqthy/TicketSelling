# Get subnet information for AZ awareness
data "aws_subnet" "selected" {
  id = var.subnet_id
}

# Manager
resource "aws_instance" "manager" {
  ami                    = var.ami_id
  instance_type          = var.instance_type
  subnet_id              = var.subnet_id
  vpc_security_group_ids = var.security_group_ids
  key_name               = var.key_name
  associate_public_ip_address = true
  availability_zone      = data.aws_subnet.selected.availability_zone

  user_data = templatefile("${path.module}/user_data_manager.sh.tpl", {})

  tags = { Name = "${var.name}-manager" }
}


# Remote-exec: fetch join token and manager private IP
resource "null_resource" "get_join_info" {
  depends_on = [aws_instance.manager]

  # Add a longer delay to ensure instance is fully initialized and SSH is available
  provisioner "local-exec" {
    command = "sleep 120"
  }
  
  # Add another check to ensure SSH is available before proceeding
  provisioner "local-exec" {
    command = "until ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 -i ${var.private_key_path} ubuntu@${aws_instance.manager.public_ip} 'echo SSH connection successful'; do echo 'Waiting for SSH connection...'; sleep 10; done"
  }

  connection {
    type        = "ssh"
    host        = aws_instance.manager.public_ip
    user        = "ubuntu"
    private_key = file(var.private_key_path)
  }

  provisioner "remote-exec" {
    inline = [
      # Wait for Docker swarm to initialize and create token file
      "while [ ! -f /tmp/swarm_worker_token.txt ]; do echo 'Waiting for swarm token file...'; sleep 10; done",
      # Format output as JSON for external data sources - ensure proper JSON formatting
      "echo '{\"token\":\"'$(cat /tmp/swarm_worker_token.txt)'\"}'  > /tmp/join_token_out.json",
      "echo '{\"ip\":\"'$(hostname -I | awk '{print $1}')'\"}'  > /tmp/manager_ip_out.json"
    ]
  }
}

# Instead of using external data sources that rely on SSH, use local variables
locals {
  # Use the manager's public IP for workers to connect to
  manager_ip = aws_instance.manager.private_ip
  
  # For the join token, we'll use a placeholder and update it in user_data
  # The actual token will be retrieved by the worker directly from the manager
  join_token = "SWARM_TOKEN_PLACEHOLDER"
}

# Worker(s)
resource "aws_instance" "worker" {
  count                  = var.worker_count
  ami                    = var.ami_id
  instance_type          = var.instance_type
  subnet_id              = var.subnet_id
  vpc_security_group_ids = var.security_group_ids
  key_name               = var.key_name
  associate_public_ip_address = true
  availability_zone      = data.aws_subnet.selected.availability_zone

  user_data = templatefile("${path.module}/user_data_worker.sh.tpl", {
    manager_ip = aws_instance.manager.private_ip
    manager_public_ip = aws_instance.manager.public_ip
  })

  tags = { Name = "${var.name}-worker-${count.index}" }
}
