
use crate::s3::S3Config;

use super::prelude::*;

#[get("/content/{file_path}")]
async fn stored_content(file_path: Path<String>, s3_config: Data<S3Config>) -> impl Responder {
    let bucket = s3_config.bucket();
    if let Err(err) = bucket {
        error!("error getting s3 bucket: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    }
    let bucket = bucket.unwrap();

    let path = file_path.into_inner();

    let data = bucket.get_object(path).await;
    if let Err(_) = data {
        return HttpResponse::NotFound().body("file not found")
    }
    let data = data.unwrap();
    let data = data.to_vec();

    let mime_type = tree_magic::from_u8(&data);
    HttpResponse::Ok().content_type(mime_type).body(data.to_vec())
}