# Linux Deployment & Update Instructions

Assumes a raspberry PI deployment for samples, but should work on most linux systems (those that use `sudo` and `systemctl`, anyway)

## Initial Setup

These instructions are for setting up the website so it runs on Linux, derived from my setup on a Raspberry Pi.

The instructions assume that the site will run on port 80 - if there is something else already using that port, you will either need to change it or change the instructions.

### Step One - Port Forwarding

On your local router, find the port forwarding section and forward both port 80 and port 443 to the linux server.

### Step Two - Dotnet Service

1. Create a dir on the server under `/var/www` called `grislygrotto`. Change the ownership of the new folder to www-data with the command `sudo chown www-data grislygrotto/`
2. Publish the project on a dev machine, targetting the linux architecture, e.g. `dotnet publish -r linux-arm -c Release`
3. Copy the published files and the Sqlite database into the linux directory. E.g., from the publish folder on the dev machine, run `scp * pi@192.168.1.69:/var/www/grislygrotto`
3. Copy the service file into systemd's service file location: `/etc/systemd/system/kestrel-grislygrotto.service`. It can be copied using scp like above.
4. Ensure it has the correct settings in an editor: `sudo nano /etc/systemd/system/kestrel-grislygrotto.service`
5. Enable and start it with the following commands: `sudo systemctl enable kestrel-grislygrotto.service` and `sudo systemctl start kestrel-grislygrotto.service`
6. Check its status to ensure it is running with the following command: `sudo systemctl status kestrel-grislygrotto.service`

You can test the above by, on the device, curling localhost:5000. The systemd config doesn't expose the site externally (to do that, change 'localhost' to '*'.

### Step Three - Setup reverse proxy via Nginx

1. Install nginx via `sudo apt-get install nginx` then ensure its started via `sudo /etc/init.d/nginx start`
2. Replace the contents of `/etc/nginx/sites-available/default` with the config in [nginx.yaml](./nginx.yaml)
3. Reload the config (which will also verify everything is typed correctly) with `sudo nginx -s reload`

Note the above config will mean the site will only show to people who come to it via http://grislygrotto.nz (no https yet until next step). To verify from inside the network, you will need to setup a hosts entry. Otherwise, come in via the internet and it should work.

### Step Four - Enable SSL via LetsEncrypt and Certbot

Follow the instructions here: [https://certbot.eff.org/](https://certbot.eff.org/). I used debian 9 (stretch) and nginx, then followed the simple instructions to configure the cert, also opting to force http traffic to https. Took less than a minute.

Note, on the PI, the debian 9 backports setup required me to create my own sources.list.d file with the 'deb ...' command inside, and I had to do this as the root user (not just via sudo ...)

Once this is done, a host entry internally and a public access externally should both end up on https://grislygrotto.nz.

## Update Instructions

To update, follow these steps:

1. Run a backup (very important!). If the backup-scripts project is not suitable or available, at least backup the database via a command like `scp pi@192.168.1.69:/var/www/grislygrotto/grislygrotto.db .`
2. Publish the latest code base for linux on a dev machine, using a command like `dotnet publish -r linux-arm -c Release`
3. SSH to the linux machine and stop the existing service. E.g. once on the box, run this command: `sudo systemctl stop kestrel-grislygrotto.service`
4. On the dev machine, copy over the newly published files: navigate to the publish out directory, and run the command `scp * pi@192.168.1.69:/var/www/grislygrotto`
5. Return to the linux machine and start the service: `sudo systemctl start kestrel-grislygrotto.service`

### Note on new files

If new files are added, they may not be marked as executable by linux. To fix this, run `sudo chmod +x *` in the directory.