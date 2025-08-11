#!/bin/sh
chgrp docker /var/run/docker.sock
exec su app -c "dotnet tilework.ui.dll"
