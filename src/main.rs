use actix_web::{HttpServer, App, web::{Data, QueryConfig}, middleware::{Logger, self}, HttpResponse, error};

use anyhow::Result;

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
    let query_cfg = get_query_cfg();
    let s3config = s3::get_s3_config()?;

    let server = HttpServer::new(move || {
        App::new()
            .wrap(middleware::Compress::default())
            .wrap(Logger::new("%a - %r - %s"))
            .app_data(Data::new(tera.clone()))
            .app_data(Data::new(s3config.clone()))
            .app_data(query_cfg.clone())
            .service(handlers::embedded::static_content)
            .service(handlers::content::stored_content)
            .service(handlers::view_posts::latest_posts)
            .service(handlers::view_posts::single_post)
            .service(handlers::comments::add_comment)
            .service(handlers::archives::archives_page)
            .service(handlers::archives::posts_for_month)
            .service(handlers::search::search_page)
            .service(handlers::about::about_page)
    });  

    server.bind("0.0.0.0:3000")?.run().await.map_err(|e| anyhow::Error::from(e))
}

fn get_query_cfg() -> QueryConfig {
    QueryConfig::default()
        .error_handler(|err, _| {
            log::error!("bad query param: {}", err);
            error::InternalError::from_response(err, HttpResponse::BadRequest().body("bad request").into()).into()
        })
}
