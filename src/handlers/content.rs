
use futures_util::StreamExt;

use crate::s3::S3Config;

use super::{prelude::*, *};

#[get("/content/{file_name}")]
async fn stored_content(file_name: Path<String>, s3_config: Data<S3Config>) -> WebResponse {
    let bucket = s3_config.bucket()?;

    let path = file_name.into_inner();

    let data = bucket.get_object(path).await?.to_vec();

    let mime_type = tree_magic::from_u8(&data);
    file(mime_type, data)
}

/// used on the editor page, but seems better here on content.rs than under editor.rs
#[post("/content/{file_name}")]
async fn upload_content(mut body: actix_web::web::Payload, file_name: Path<String>, s3_config: Data<S3Config>, session: Session) -> WebResponse {
    if session.get::<String>("current_user")?.is_none() {
        return Err(WebError::Forbidden);
    }

    let mut bytes = actix_web::web::BytesMut::new();
    while let Some(item) = body.next().await {
        let item = item?;
        bytes.extend_from_slice(&item);
    }

    let data = bytes.to_vec();
    if bytes.len() > 1024*1024 {
        return Err(WebError::BadRequest("file size too large".into()))
    }

    let mime_type = tree_magic::from_u8(&data);
    if !mime_type.starts_with("image/") && mime_type != "video/mp4" && mime_type != "application/zip" {
        return Err(WebError::BadRequest("mime type not allowed".into()))
    }

    let bucket = s3_config.bucket()?;
    bucket.put_object(file_name.into_inner(), &data).await?;

    accepted("content uploaded successfully".into())
}
