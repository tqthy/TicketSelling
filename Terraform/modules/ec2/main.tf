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

  # Add a delay to ensure user_data script has completed
  provisioner "local-exec" {
    command = "sleep 60"
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
      # Format output as JSON for external data sources
      "echo '{\"token\":'\"$(cat /tmp/swarm_worker_token.txt)\"'}' > /tmp/join_token_out.json",
      "echo '{\"ip\":'\"$(hostname -I | awk '{print $1}')\"'}' > /tmp/manager_ip_out.json"
    ]
  }
}

data "external" "join_token" {
  depends_on = [null_resource.get_join_info]

  program = [
    "bash", "-c",
    "ssh -o StrictHostKeyChecking=no -i ${var.private_key_path} ubuntu@${aws_instance.manager.public_ip} 'cat /tmp/join_token_out.json'"
  ]
}

data "external" "manager_ip" {
  depends_on = [null_resource.get_join_info]

  program = [
    "bash", "-c",
    "ssh -o StrictHostKeyChecking=no -i ${var.private_key_path} ubuntu@${aws_instance.manager.public_ip} 'cat /tmp/manager_ip_out.json'"
  ]
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
    join_token = data.external.join_token.result["token"]
    manager_ip = data.external.manager_ip.result["ip"]
  })

  tags = { Name = "${var.name}-worker-${count.index}" }
}
