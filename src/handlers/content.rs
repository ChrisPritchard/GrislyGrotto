use std::env;

use s3::{creds::Credentials, Bucket, Region};

use super::prelude::*;

#[get("/content/{file_path}")]
async fn stored_content(file_path: Path<String>) -> impl Responder {

    let bucket_name = env::var("AWS_BUCKET_NAME").unwrap();
    let region: Region = env::var("AWS_REGION").unwrap().parse().unwrap();
    let access_key = env::var("AWS_ACCESS_KEY_ID").unwrap();
    let secret_key = env::var("AWS_SECRET_ACCESS_KEY").unwrap();

    let credentials = Credentials::new(Some(&access_key), Some(&secret_key), None, None, None).unwrap();
    let bucket = Bucket::new(&bucket_name, region, credentials).unwrap();
    let path = file_path.into_inner();

    let data = bucket.get_object(path).await.unwrap();

    HttpResponse::Ok().content_type("image/jpeg").body(data.to_vec())
}