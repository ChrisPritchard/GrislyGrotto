use actix_web::{HttpServer, App, web::{Data, QueryConfig}, middleware::Logger, HttpResponse, error};

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
            .wrap(Logger::new("%a - %r - %s"))
            .app_data(Data::new(tera.clone()))
            .app_data(query_cfg.clone())
            .service(handlers::latest)
            .service(handlers::single)
            .service(handlers::add_comment)
            .service(handlers::static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
