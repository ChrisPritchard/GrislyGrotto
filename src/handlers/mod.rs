
mod static_files;
mod view_posts;

pub use static_files::static_content;
pub use view_posts::latest;
pub use view_posts::single;

mod prelude {
    pub use actix_web::{Responder, get, web::*, HttpResponse};
    pub use tera::Tera;
}