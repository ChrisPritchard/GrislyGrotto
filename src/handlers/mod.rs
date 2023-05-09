use std::fmt::Display;

use actix_web::{error, http::header, HttpResponse};

pub mod style;
pub mod content;
pub mod view_posts;
pub mod comments;
pub mod archives;
pub mod search;
pub mod about;
pub mod account;
pub mod editor;

mod prelude {
    pub use actix_web::{get, post, web::{Data, Query, Path, Form}};
    pub use tera::Tera;
    pub use serde::Deserialize;
    pub use log::error;
    pub use actix_session::Session;
    pub use std::collections::HashSet;
}

use prelude::*;
use serde::Serialize;

fn default_tera_context(session: &actix_session::Session) -> anyhow::Result<tera::Context> {
    let mut context = tera::Context::new();
    
    let style: String = session.get("style")?.unwrap_or("light".into());
    context.insert("style", &style.clone());
    
    let current_user: Option<String> = session.get("current_user")?;
    if let Some(username) = current_user {
        context.insert("current_user", &username.clone());
    }
    
    Ok(context)
}

type WebResponse = Result<HttpResponse, WebError>;

fn redirect(address: String) -> WebResponse {
    Result::Ok(HttpResponse::SeeOther().insert_header((header::LOCATION, address)).finish())
}

fn ok(body: String) -> WebResponse {
    Result::Ok(HttpResponse::Ok().body(body))
}

fn json<T: Serialize>(body: T) -> WebResponse {
    Result::Ok(HttpResponse::Ok().json(body))
}

fn accepted(body: String) -> WebResponse {
    Result::Ok(HttpResponse::Accepted().body(body))
}

fn file(mime_type: String, bytes: Vec<u8>) -> WebResponse {
    Result::Ok(HttpResponse::Ok().content_type(mime_type).body(bytes))
}

#[derive(Debug)]
enum WebError {
    NotFound, BadRequest(String), Forbidden, ServerError(anyhow::Error)
}

impl Display for WebError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self)
    }
}

impl error::ResponseError for WebError {
    fn error_response(&self) -> HttpResponse {
        match self {
            WebError::NotFound => error::ErrorNotFound("404 not found").into(),
            WebError::BadRequest(m) => {
                error::ErrorNotFound(m.clone()).into()
            }
            WebError::Forbidden => error::ErrorNotFound("403 forbidden").into(),
            WebError::ServerError(e) => {
                error!("{}", e);
                error::ErrorInternalServerError("500 internal server error").into()
            }
        }
    }
}

impl From<anyhow::Error> for WebError {
    fn from(err: anyhow::Error) -> Self {
        Self::ServerError(err)
    }
}

impl From<s3::error::S3Error> for WebError {
    fn from(err: s3::error::S3Error) -> Self {
        let err = anyhow::Error::from(err);
        Self::ServerError(err)
    }
}

impl From<actix_web::error::PayloadError> for WebError {
    fn from(err: actix_web::error::PayloadError) -> Self {
        let err = anyhow::Error::from(err);
        Self::ServerError(err)
    }
}

impl From<actix_session::SessionGetError> for WebError {
    fn from(err: actix_session::SessionGetError) -> Self {
        let err = anyhow::Error::from(err);
        Self::ServerError(err)
    }
}

impl From<tera::Error> for WebError {
    fn from(err: tera::Error) -> Self {
        let err = anyhow::Error::from(err);
        Self::ServerError(err)
    }
}