
mod static_files;
mod view_posts;

pub use static_files::static_content;
pub use view_posts::latest;

mod prelude {
    pub use actix_web::{Responder, get, web::{self, Data}, HttpResponse};
    pub use tera::Tera;
}