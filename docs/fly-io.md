# Deploying on Fly.IO

All fly commands will take the app of the folder they are currently run in, or an additional argument `-a grislygrotto`. This is reflected in some commands below and not others.

1. Ensure you have an account with Fly.IO, and the `flyctl` CLI tool installed.
    - the tool can be installed on windows with `iwr https://fly.io/install.ps1 -useb | iex`
    - on linux with `curl -L https://fly.io/install.sh | sh`
2. Create the app, using `fly apps create`. Use the name grislygrotto and syd region
3. Create a volume to host the database: `fly volume create grislygrotto_db`. Put it in the syd region.
4. From the root of the project, run `fly launch --copy-config --reuse-app --no-deploy`
    - this will build the docker image but not deploy it
    - `--reuse-app` will put it in the same place as the existing deployment, if one exists
    - `--copy-config` doesn't prompt for copying the existing config
    - `--no-deploy` will not deploy the app, allowing you to set the env vars
    - `--no-cache` can be appended to not use docker caches - should not be used unless the docker file radically changes
5. Add secrets for AWS & the session key, the following need to be set:
    ```
    AWS_REGION=ap-southeast-2
    AWS_ACCESS_KEY_ID=[REDACTED]
    AWS_SECRET_ACCESS_KEY=[REDACTED]
    AWS_BUCKET_NAME=grislygrotto-content
    SESSION_KEY=[REDACTED]
    ```
    The bucket name and region will likely not change.
    Secrets can be set with `fly secrets set AWS_REGION=ap-southeast-2 AWS_ACCESS_KEY_ID=[REDACTED] AWS_SECRET_ACCESS_KEY=[REDACTED] AWS_BUCKET_NAME=grislygrotto-content SESSION_KEY=[REDACTED]`.
    Alternately, each secret can be set individually, e.g. `fly secrets set AWS_REGION=ap-southeast-2`.

    Note the SESSION_KEY must be 64 bytes wrong and very random. Using a SHA2-256 hash of something randomly generated will do it.
    If not specified, the session_key will be randomly generated on every start, which is problematic for fly.io as the app gets stopped when there is no activity.
6. Deploy the app using `fly deploy` - this is required so an actual VM is started, in order to access and upload the database
7. Open a sftp shell and upload the latest version of `grislygrotto.db` to `/mnt/db`:
    - use `fly sftp shell -a grislygrotto` to connect over sftp - you might need to run `fly ssh console -a grislygrotto` first if auth fails, not sure
    - use `put [localpath] /mnt/db/grislygrotto.db` to upload the db
    - then `exit`
8. Finally, check all is well with `fly status` to see logs, or `fly open` to open the site in a browser

## DNS Setup

These instructions are to set it up with the `grislygrotto.nz` domain.

1. Get the IPs of the app with `flyctl ips list -a grislygrotto`
2. Update the DNS server with A and AAAA records
3. Run `flyctl certs create -a grislygrotto grislygrotto.nz` to create a valid HTTPS cert

The following command can show these being created: `flyctl certs show -a grislygrotto grislygrotto.nz`

## Database backups

To backup the database, first ensure the fly.io instance is started with `curl -I grislygrotto.nz`

Then, the flyctl command line tool can be used to backup the database locally: `fly sftp get /mnt/db/grislygrotto.db grislygrotto.db -a grislygrotto`

- `-a` specifies the app to use
- the first argument after get is the remote location to retrieve
- the second argument is the local location to save 

## Updates (code only)

Should be a simple matter of running `fly launch --copy-config --reuse-app` - default options (enter, enter, enter), but deploy when asked.