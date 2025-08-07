use anyhow::anyhow;
use futures_util::StreamExt;
use image::{imageops::FilterType, DynamicImage, ImageFormat};
use std::io::Cursor;

use crate::s3::S3Config;

use super::{prelude::*, *};

#[get("/content/{file_name}")]
async fn stored_content(file_name: Path<String>, s3_config: Data<S3Config>) -> WebResponse {
    let bucket = s3_config.bucket()?;

    let path = file_name.into_inner();

    let data = bucket.get_object(&path).await?.to_vec();

    let mime_type = mime_type(&data, path.ends_with(".webp"));
    file(mime_type, data)
}

/// used on the editor page, but seems better here on content.rs than under editor.rs
#[post("/content/{file_name}")]
async fn upload_content(
    mut body: actix_web::web::Payload,
    file_name: Path<String>,
    s3_config: Data<S3Config>,
    session: Session,
) -> WebResponse {
    if session.get::<String>("current_user")?.is_none() {
        return Err(WebError::Forbidden);
    }

    let mut bytes = actix_web::web::BytesMut::new();
    while let Some(item) = body.next().await {
        let item = item?;
        bytes.extend_from_slice(&item);
    }

    let mut data = bytes.to_vec();

    let mime_type = mime_type(&data, file_name.ends_with(".webp"));
    if !mime_type.starts_with("image/")
        && mime_type != "video/mp4"
        && mime_type != "application/zip"
    {
        return Err(WebError::BadRequest("mime type not allowed".into()));
    }

    let max_size_bytes = 500 * 1024;

    if mime_type.starts_with("image/") {
        if !is_webp(&data) || data.len() > max_size_bytes {
            match image::load_from_memory(&data) {
                Ok(img) => match process_image(img, max_size_bytes).await {
                    Ok(processed_data) => data = processed_data,
                    Err(e) => {
                        log::error!("Failed to process image: {}", e);
                        return Err(WebError::ServerError(anyhow!("Failed to process image")));
                    }
                },
                Err(e) => {
                    return Err(WebError::BadRequest(format!("Invalid image: {}", e).into()));
                }
            }
        }
    }

    let bucket = s3_config.bucket()?;
    bucket.put_object(file_name.into_inner(), &data).await?;

    accepted("content uploaded successfully".into())
}

async fn process_image(
    mut img: DynamicImage,
    max_size_bytes: usize,
) -> Result<Vec<u8>, anyhow::Error> {
    let mut quality: f32 = 1.0; // Start with decent quality
    let output;

    let initial_width = img.width() as f32;
    let initial_height = img.height() as f32;

    loop {
        let mut webp_data = Vec::new();

        if quality != 1.0 {
            img = img.resize(
                (initial_width * quality) as u32,
                (initial_height * quality) as u32,
                FilterType::Lanczos3,
            )
        }
        // Encode with current quality setting
        img.write_to(&mut Cursor::new(&mut webp_data), ImageFormat::WebP)?;

        // Check if within size limit
        if webp_data.len() <= max_size_bytes || quality <= 0.1 {
            output = webp_data;
            break;
        }

        // Reduce quality for next attempt
        quality *= 0.9; // Reduce quality by 10%
    }

    Ok(output)
}

fn is_webp(data: &[u8]) -> bool {
    // WebP signature check (RIFF....WEBP)
    data.len() > 12 && &data[0..4] == b"RIFF" && &data[8..12] == b"WEBP"
}
