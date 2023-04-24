use actix_web::{HttpServer, App, web::{Data, QueryConfig}, middleware::{Logger, self}, HttpResponse, error};

mod model;
mod data;
mod handlers;
mod templates;

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let tera = templates::template_engine();

    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));

    let query_cfg = QueryConfig::default()
        .error_handler(|err, _| {
            log::error!("bad query param: {}", err);
            error::InternalError::from_response(err, HttpResponse::BadRequest().body("bad request").into()).into()
        });

    let server = HttpServer::new(move || {
        App::new()
            .wrap(middleware::Compress::default())
            .wrap(Logger::new("%a - %r - %s"))
            .app_data(Data::new(tera.clone()))
            .app_data(query_cfg.clone())
            .service(handlers::view_posts::latest_posts)
            .service(handlers::view_posts::single_post)
            .service(handlers::comments::add_comment)
            .service(handlers::embedded::static_content)
            .service(handlers::archives::archives_page)
            .service(handlers::search::search_page)
            .service(handlers::about::about_page)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
