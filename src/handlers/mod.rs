
pub mod style;
pub mod content;
pub mod view_posts;
pub mod comments;
pub mod archives;
pub mod search;
pub mod about;
pub mod login;

mod prelude {
    pub use actix_web::{Responder, get, post, web::*, HttpResponse};
    pub use tera::Tera;
    pub use serde::Deserialize;
    pub use log::error;
    pub use actix_session::Session;
}

fn default_tera_context(session: actix_session::Session) -> tera::Context {
    let mut context = tera::Context::new();
    
    let style: String = session.get("style").unwrap().unwrap_or("light".into());
    context.insert("style", &style.clone());
    
    context
}