use actix_web::{HttpServer, App, web::Data, middleware::Logger};

mod model;
mod data;
mod handlers;
mod templates;

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let tera = templates::template_engine();

    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));

    let server = HttpServer::new(move || {
        App::new()
            .wrap(Logger::new("%a %{User-Agent}i"))
            .app_data(Data::new(tera.clone()))
            .service(handlers::latest)
            .service(handlers::single)
            .service(handlers::add_comment)
            .service(handlers::static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
