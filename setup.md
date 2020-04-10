# Setup instructions

These instructions detail the full steps to get this running in a semi permanent fashion with SSL and a custom domain on a linux server.

They involve:

1. Compiling the application
2. Getting it running on a destination machine
3. Persistence via systemd
4. A reverse proxy for SSL purposes
5. Setting up lets encrypt

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
    - all files under /static to the static folder on the destination machine (should just be three)

5. ssh back to the machine, and run the grislygrotto.so file. All going well, you should get the following message:

```
The Grisly Grotto has started!
listening locally at port :3000
```

You can test this by opening another session on the machine and curling the address, or trying to reach the machine externally via :3000 (assuming there is no firewall or security group in the way).