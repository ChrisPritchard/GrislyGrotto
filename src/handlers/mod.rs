
pub mod embedded;
pub mod view_posts;
pub mod comments;
pub mod archives;
pub mod search;
pub mod about;

mod prelude {
    pub use actix_web::{Responder, get, post, web::*, HttpResponse};
    pub use tera::Tera;
    pub use serde::Deserialize;
    pub use log::error;
}