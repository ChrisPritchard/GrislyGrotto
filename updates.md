# Update & Error Debugging instructions

To perform an update:

1. Perform a backup of the site. At the very least, copy the existing grislygrotto.db file somewhere safe.

2. Ensure the code has re-embedded all static assets and views, via the arg `-embed`.

3. In the `/src` folder, compile `grislygrotto.so`. For linux (x64) this would be:

    `env CGO_ENABLED=1 GOOS=linux GOARCH=amd64 go build -o grislygrotto.so`

4. Copy the .so file onto the destination server, into a holding folder (e.g. `cd ~`) via `scp` or equivalent.

5. On the destination server, if using a service like that described in `setup.md`, stop it via something like:

    `sudo systemctl stop grislygrotto.service`

6. Use `mv` or `sudo mv` to copy the new .so over the old .so, e.g.

    `sudo mv ~/grislygrotto.so /var/www/grislygrotto/`

    **NOTE**: if also updating the database for whatever reason (change in schema, or content updates via sql etc), then after sudoing it into place, you should also run `sudo chown www-data grislygrotto.db`, otherwise the site will not be able to update it.

7. Restart the service stopped in step 5 via:

    `sudo systemctl start grislygrotto.service`

8. Test release was successful, then log off the machine.

Debugging issues:

- You can get any messages fired during startup via:

    `sudo systemctl status grislygrotto.service`

- Alternatively, there might be logs in the nginx logs

- Or in the system error logs.

- Full output stack traces can be seen via the following command, if thrown:

    `journalctl -u grislygrotto.service`

### Renewing the cert

Should just be a matter of, on or near the day of expiry, ssh'ing in and running `sudo certbot renew`

### Mass text updates

If some text in the database becomes out of date, e.g. if the image path changes and all posts need to be updated to reflect this, you could do:

1. backup the database
2. make a copy to edit
3. open with the sqlite3 command line tool
4. run a query like: `UPDATE POSTS SET Content = REPLACE(Content,'old','new');`
5. exit with ctrl-d after checking updates have been made via a select
6. copy the new db back to the server into a temp location
7. turn off gg
8. copy the db in place, overwriting the old db
9. chown the db back to www-data
10. start gg

easy
