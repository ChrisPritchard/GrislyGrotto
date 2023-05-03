
use crate::s3::S3Config;

use super::{prelude::*, *};

#[get("/content/{file_path}")]
async fn stored_content(file_path: Path<String>, s3_config: Data<S3Config>) -> WebResponse {
    let bucket = s3_config.bucket()?;

    let path = file_path.into_inner();

    let data = bucket.get_object(path).await?.to_vec();

    let mime_type = tree_magic::from_u8(&data);
    file(mime_type, data)
}
