
mod static_files;
mod view_posts;
mod comments;

pub use static_files::static_content;
pub use view_posts::latest;
pub use view_posts::single;
pub use comments::add_comment;

mod prelude {
    pub use actix_web::{Responder, get, post, web::*, HttpResponse};
    pub use tera::Tera;
    pub use serde::Deserialize;
    pub use log::error;
}