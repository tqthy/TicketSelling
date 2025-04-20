#!/bin/bash
# EC2 User Data script to install Docker using the convenience script from get.docker.com

yum update -y 


echo "Starting Docker installation..."

# --- Download and execute the Docker installation script ---
# -f: Fail silently on server errors
# -s: Silent mode (don't show progress)
# -S: Show error messages even in silent mode
# -L: Follow redirects
# -o: Output to a file (get-docker.sh)
curl -fsSL https://get.docker.com -o get-docker.sh
if [ $? -ne 0 ]; then
    echo "Failed to download get-docker.sh script. Exiting."
    exit 1
fi

# Execute the downloaded script
# The script automatically detects the distribution and installs Docker
sh get-docker.sh
if [ $? -ne 0 ]; then
    echo "Docker installation script failed. Exiting."
    exit 1
fi

echo "Docker installation script executed successfully."

# --- Add the default user to the 'docker' group ---
# Replace 'ec2-user' with the correct default username for your AMI (e.g., 'ubuntu', 'admin').
USERNAME="ec2-user"

if id "$USERNAME" &>/dev/null; then
  echo "Adding user '$USERNAME' to the docker group..."
  usermod -aG docker $USERNAME
  if [ $? -ne 0 ]; then
      echo "Failed to add user '$USERNAME' to docker group."
  else
      echo "User '$USERNAME' added to docker group. Remember to log out and back in or reboot for changes to apply."
  fi
else
  echo "Default user '$USERNAME' not found. Skipping adding user to docker group."
fi

echo "User data script finished."

rm get-docker.sh

