# Setup

## On Linux

1. Create a dir on the server under `/var/www` called grislygrotto works. Change the ownership of the new folder to www-data with the command `sudo chown www-data grislygrotto/`
2. Publish the project on a dev machine, targetting the linux architecture, e.g. `dotnet publish -r linux-arm -c Release`
3. Copy the published files and the Sqlite database into the linux directory. E.g., from the publish folder on the dev machine, run `scp * pi@192.168.1.69:/var/www/grislygrotto`
3. Copy the service file into systemd's service file location: `/etc/systemd/system/kestrel-grislygrotto.service`. It can be copied using scp like above.
4. Ensure it has the correct settings in an editor: `sudo nano /etc/systemd/system/kestrel-grislygrotto.service`
5. Enable and start it with the following commands: `sudo systemctl enable kestrel-grislygrotto.service` and `sudo systemctl start kestrel-grislygrotto.service`
6. Check its status to ensure it is running with the following command: `sudo systemctl status kestrel-grislygrotto.service`
