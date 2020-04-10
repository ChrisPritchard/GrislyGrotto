# Setup instructions

These instructions detail the full steps to get this running in a semi permanent fashion with SSL and a custom domain on a linux server.

They involve:

1. Compiling the application
2. Getting it running on a destination machine
3. Persistence via systemd
4. Sorting custom domains
5. Setting up nginx reverse proxy
6. Setting up lets encrypt

> **NOTE** If you want to use ssl/tls via lets encrypt, then it is *strongly* recommended you use an OS that is natively supported by certbot. Go [here](https://certbot.eff.org/) and see if its one of the options.
>
> Don't be me and get all the way through this with Amazon Linux 2, only to have to nuke it and start again with Ubuntu because certbot doesn't support AL2.

## Compiling

To compile you need access to the source code and `go` installed somewhere. Its slightly more complicated than normal with gg because it uses cgo. Accordingly, you also need some flavour of `gcc` installed that will work with the architecture you are targeting.

For regular x64 linux machines, the following should be all you need to run (if you don't have gcc, its easy enough to get running):

`env CGO_ENABLED=1 GOOS=linux GOARCH=amd64 go build -o grislygrotto.so`

This has to be run from the folder with the .go source files, just so they're picked up easier.

If targeting ARM however, e.g. if planning on deploying to a pi, you need to run:

`env CGO_ENABLED=1 CC=arm-linux-gnueabi-gcc GOOS=linux GOARCH=arm GOARM=5 go build -o grislygrotto.so`

That compiler in there can be installed by installing `arm-linux-gnueabi-gcc` and its dependencies.

## Getting the site running

All this requires is scp'ing the compiled .so file, the sqlite3 .db file, the static files under /static, and a .secret file used for encryption.

1. ssh to the target machine and make a folder somewhere.
2. create a .secret file in it 16 characters long, e.g. `echo 1234567890123456 > .secret`
3. make an empty static folder: `mkdir static`
4. exit `ssh` and use `scp` to copy over:

    - the grislygrotto.so file
    - the grislygrotto.db file
    - all files under /static to the static folder on the destination machine (should just be four files)

5. ssh back to the machine, and run the grislygrotto.so file. All going well, you should get the following message:

```
The Grisly Grotto has started!
listening locally at port :3000
```

You can test this by opening another session on the machine and curling the address, or trying to reach the machine externally via :3000 (assuming there is no firewall or security group in the way).

## Persistence via systemd

Running the site manually from the command line isn't a good idea. Instead we should move it somewhere intelligent and then use a systemd job to start it.

1. Create a folder to host it, e.g. `/var/www/grislygrotto/`. If `www` doesnt exist, create it (probably with `sudo mkdir`). Move all the required files from the previous step into this folder.
2. Create the www-data user (if not present) and give them the www folder. `sudo useradd -r www-data` will create the user, and then `sudo chown -R www-data /var/www` will give them ownership of the www and its contents.
3. Navigate to `/etc/systemd/system/` and create a new service file with nano, if you have it. E.g. `sudo nano grislygrotto.service`. Place in it the following content:

    ```
    [Unit]
    Description=GrislyGrotto Website

    [Service]
    WorkingDirectory=/var/www/grislygrotto
    ExecStart=/var/www/grislygrotto/grislygrotto.so -db ./grislygrotto.db -url ":5000"
    Restart=always
    RestartSec=10
    KillSignal=SIGINT
    SyslogIdentifier=grislygrotto-web
    User=www-data

    [Install]
    WantedBy=multi-user.target
    ```

4. Start the service using the following commands: 

    `sudo systemctl enable grislygrotto.service`

    `sudo systemctl start grislygrotto.service`

You can check its status (which should show the same message as the previous step, but with port 5000) via the following command `sudo systemctl status grislygrotto.service`

## Sorting custom domains

At this point its a good idea to get the custom domain you might be using, e.g. grislygrotto.nz, to point at your server. The next few steps with nginx and lets encrypt will be easier if this is working.

Generally speaking this should just involve setting your domain register's A records to point at your server's IP Address. If using something like Amazon EC2, you might want to assign an Elastic IP for this.

## Setting up nginx reverse proxy

Nginx provides a nice configurable front end, and also makes it easier to setup lets encrypt which has nice defaults for nginx.

1. Install nginx. How you do this differs by platform. You can check its working by browsing to port 80 (again, check you are allowing port 80 e.g. via a security group): nginx should have a default landing page.

    > A common way to install nginx is via `sudo apt-get install nginx`

2. Reconfigure nginx to point to the gg server, by editing nginx.conf and replacing the server block with the gg one:

    a. find the server { ... } section in nginx.conf

    > if there isn't a section in here, check if `sites-available/default` exists, and use that file instead if so

    It should start something like this:

    ```
    server {
        listen       80 default_server;
        listen       [::]:80 default_server;
        server_name  _;
        root         /usr/share/nginx/html;
    ```

    b. Replace the whole server block with:

    ```
    server {
        listen        80;
        server_name   server_name grislygrotto.nz *.grislygrotto.nz;
        location / {
            proxy_pass         http://localhost:5000;
            proxy_http_version 1.1;
            proxy_set_header   Upgrade $http_upgrade;
            proxy_set_header   Connection keep-alive;
            proxy_set_header   Host $host;
            proxy_cache_bypass $http_upgrade;
            proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header   X-Forwarded-Proto $scheme;
        }
    }
    ```

3. Reload nginx: `sudo nginx -s reload`

All going well, browsing to your domain should show the site. Don't forget to delete any rules that allowed port 3000 or 5000 now! Shouldn't need them.

## Setting up lets encrypt

Follow the instructions here: https://certbot.eff.org/. If you are using a supported OS (listed at the top as a suggest pre-req) this should be trivial:

- Pick nginx and your os, then followed the simple instructions to configure the cert
- When prompted, opt to force http traffic to https.

Should take less than a minute, and then can test (once/if you have configured incoming 443 traffic through the security group or your firewall) that the site is hosted properly via https://

> Note, if using a domain like grislygrotto.nz, but *also* www.grislygrotto.nz, you might need to modify the nginx default file and ensure that both domains are in there. *.grislygrotto.nz instead of www.grislygrotto.nz, might result in certbot creating just a cert for grislygrott.nz, which will mean that visiting the www. address will result in a cert warning.
>
> The solution is to specify the www. domain specifically, then rerun the cert bot tool and select both domains (if it detects both, hopefully). If you have already run it before it might prompt to 'expand' the existing cert, which is an easy option to fix things up.