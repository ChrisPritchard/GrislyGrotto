use anyhow::{anyhow, Result};
use futures_util::StreamExt;
use image::{imageops::FilterType, DynamicImage};
use webp::Encoder;

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
    let max_width_pixels = 2000;

    if mime_type.starts_with("image/") && !file_name.contains("anim") {
        if !is_webp(&data) || data.len() > max_size_bytes {
            match image::load_from_memory(&data) {
                Ok(img) => match process_image(img, max_size_bytes, max_width_pixels).await {
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
    max_width_pixels: usize,
) -> Result<Vec<u8>> {
    // Resize the image to be within maximum size
    let width = img.width() as f32;
    if width > max_width_pixels as f32 {
        let ratio = (max_width_pixels as f32) / width;
        let height = img.height() as f32;
        img = img.resize(
            (width * ratio) as u32,
            (height * ratio) as u32,
            FilterType::Lanczos3,
        );
    }

    let mut quality = 80.0;
    loop {
        let webp_data = encode_image(&img, quality)?;

        // Check if within size limit
        if webp_data.len() <= max_size_bytes || quality <= 0.1 {
            return Ok(webp_data);
        }

        // Reduce quality for next attempt
        quality *= 0.9; // Reduce quality by 10%
    }
}

// This was a lot harder than it needed to be
fn encode_image(img: &DynamicImage, quality: f32) -> Result<Vec<u8>> {
    let data = img.to_rgba8();
    let result = {
        let encoder = Encoder::from_rgba(&data, img.width(), img.height());
        let encoded = encoder.encode(quality);
        encoded.to_vec()
    };
    Ok(result)
}

fn is_webp(data: &[u8]) -> bool {
    // WebP signature check (RIFF....WEBP)
    data.len() > 12 && &data[0..4] == b"RIFF" && &data[8..12] == b"WEBP"
}
