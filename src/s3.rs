use std::env;

use s3::{Region, creds::Credentials, Bucket};
use anyhow::Result;

#[derive(Clone)]
pub struct S3Config {
    region_name: String,
    access_key: String,
    secret_key: String,
    bucket_name: String
}

impl S3Config {
    pub fn bucket(&self) -> Result<Bucket> {
        let region: Region = self.region_name.parse()?;
        let credentials = Credentials::new(Some(&self.access_key), Some(&self.secret_key), None, None, None)?;
        Ok(Bucket::new(&self.bucket_name, region, credentials)?)
    }
}

pub fn get_s3_config() -> Result<S3Config> {
    Ok(S3Config {
        bucket_name: env::var("AWS_BUCKET_NAME")?,
        region_name: env::var("AWS_REGION")?,
        access_key: env::var("AWS_ACCESS_KEY_ID")?,
        secret_key: env::var("AWS_SECRET_ACCESS_KEY")?,
    })
}