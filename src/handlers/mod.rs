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

mod prelude {
    pub use actix_web::{get, post, web::{Data, Query, Path, Form}};
    pub use tera::Tera;
    pub use serde::Deserialize;
    pub use log::error;
    pub use actix_session::Session;
    pub use std::collections::HashSet;
}

use prelude::*;

fn default_tera_context(session: &actix_session::Session) -> tera::Context {
    let mut context = tera::Context::new();
    
    let style: String = session.get("style").unwrap().unwrap_or("light".into());
    context.insert("style", &style.clone());
    
    let current_user: Option<String> = session.get("current_user").unwrap();
    if let Some(username) = current_user {
        context.insert("current_user", &username.clone());
    }
    
    context
}

type WebResponse = Result<HttpResponse, WebError>;

fn Redirect(address: String) -> WebResponse {
    Result::Ok(HttpResponse::SeeOther().insert_header((header::LOCATION, address)).finish())
}

fn Ok(body: String) -> WebResponse {
    Result::Ok(HttpResponse::Ok().body(body))
}

fn Accepted(body: String) -> WebResponse {
    Result::Ok(HttpResponse::Accepted().body(body))
}

fn File(mime_type: String, bytes: Vec<u8>) -> WebResponse {
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
                let m = format!("400 {m}");
                error::ErrorNotFound(m).into()
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