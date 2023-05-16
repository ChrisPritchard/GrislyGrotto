# Deploying on Fly.IO

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
5. Add secrets for AWS, the following need to be set:
    ```
    AWS_REGION=ap-southeast-2
    AWS_ACCESS_KEY_ID=[REDACTED]
    AWS_SECRET_ACCESS_KEY=[REDACTED]
    AWS_BUCKET_NAME=grislygrotto-content
    ```
    The bucket name and region will likely not change.
    Secrets can be set with `fly secrets set AWS_REGION=ap-southeast-2 AWS_ACCESS_KEY_ID=[REDACTED] AWS_SECRET_ACCESS_KEY=[REDACTED] AWS_BUCKET_NAME=grislygrotto-content`.
    Alternately, each secret can be set individually, e.g. `fly secrets set AWS_REGION=ap-southeast-2`.
6. Deploy the app using `fly deploy` - this is required so an actual VM is started, in order to access and upload the database
7. Open a sftp shell and upload the latest version of `grislygrotto.db` to `/mnt/db`:
    - use `fly sftp shell` to connect over sftp - you might need to run `fly ssh console` first if auth fails, not sure
    - use `put [localpath] /mnt/db/grislygrotto.db` to upload the db
    - then `exit`
8. Finally, check all is well with `fly status` to see logs, or `fly open` to open the site in a browser