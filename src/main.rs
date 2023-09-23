use std::env;

use actix_session::{SessionMiddleware, storage::CookieSessionStore, config::PersistentSession};
use actix_web::{HttpServer, App, web::{Data, QueryConfig}, middleware::{Logger, self}, HttpResponse, error, cookie::{Key, time::Duration}, http::header};

use anyhow::Result;

mod embedded;
mod model;
mod data;
mod handlers;
mod templates;
mod s3;

#[tokio::main]
async fn main() -> Result<()> {

    dotenv::dotenv().ok();

    let tera = templates::template_engine();
    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));
    
    let s3_config = s3::get_s3_config()?;

    let specified_key = env::var("SESSION_KEY");
    let session_key = if specified_key.is_ok() { 
        let bytes: Vec<u8> = specified_key.unwrap().bytes().collect();
        Key::try_from(&bytes[..])? 
    } 
    else { Key::generate() };
    let query_extractor_cfg = get_query_extractor_cfg();

    let server = HttpServer::new(move || {
        App::new()
            .wrap(middleware::Compress::default())
            .wrap(Logger::new("%r %s - %{r}a %{User-Agent}i"))
            .wrap(middleware::DefaultHeaders::new()
                .add((header::CONTENT_SECURITY_POLICY, "default-src 'self';style-src 'self' 'unsafe-inline' cdnjs.cloudflare.com;script-src 'self' cdnjs.cloudflare.com;frame-src 'self' *.youtube.com chrispritchard.github.io;frame-ancestors 'none'"))
                .add((header::X_CONTENT_TYPE_OPTIONS, "nosniff"))
                .add((header::REFERRER_POLICY, "same-origin"))
                .add((header::PERMISSIONS_POLICY, "microphone=(), geolocation=(), camera=(), usb=(), serial=()"))
                .add((header::STRICT_TRANSPORT_SECURITY, "max-age=31536000; includeSubDomains")))
            .wrap(SessionMiddleware::builder(CookieSessionStore::default(), session_key.clone())
                .session_lifecycle(
                    PersistentSession::default().session_ttl(Duration::days(90)))
                .build())
            .app_data(Data::new(tera.clone()))
            .app_data(Data::new(s3_config.clone()))
            .app_data(query_extractor_cfg.clone())
            .service(handlers::style::set_style)
            .service(embedded::robots)
            .service(embedded::static_content)
            .service(handlers::content::stored_content)
            .service(handlers::content::upload_content)
            .service(handlers::view_posts::latest_posts)
            .service(handlers::view_posts::single_post)
            .service(handlers::view_posts::delete_post)
            .service(handlers::comments::add_comment)
            .service(handlers::comments::raw_comment_content)
            .service(handlers::comments::edit_comment)
            .service(handlers::comments::delete_comment)
            .service(handlers::archives::archives_page)
            .service(handlers::archives::posts_for_month)
            .service(handlers::search::search_page)
            .service(handlers::about::about_page)
            .service(handlers::account::login_page)
            .service(handlers::account::try_login)
            .service(handlers::account::logout)
            .service(handlers::account::account_details)
            .service(handlers::account::update_display_name)
            .service(handlers::account::update_profile_image)
            .service(handlers::account::update_password)
            .service(handlers::editor::similar_title_exists)
            .service(handlers::editor::new_post_page)
            .service(handlers::editor::create_new_post)
            .service(handlers::editor::edit_post_page)
            .service(handlers::editor::update_post)
    });  

    server
        .worker_max_blocking_threads(2048)
        .bind("[::]:3000")?.run().await.map_err(|e| anyhow::Error::from(e))
}

fn get_query_extractor_cfg() -> QueryConfig {
    QueryConfig::default()
        .error_handler(|err, _| {
            log::error!("bad query param: {}", err);
            error::InternalError::from_response(err, HttpResponse::BadRequest().body("bad request").into()).into()
        })
}
