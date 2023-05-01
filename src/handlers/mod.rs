
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