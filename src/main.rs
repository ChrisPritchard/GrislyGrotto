use actix_web::{HttpServer, App, web::Data};

mod model;
mod data;
mod handlers;
mod templates;

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let tera = templates::template_engine();

    let server = HttpServer::new(move || {
        App::new()
            .app_data(Data::new(tera.clone()))
            .service(handlers::latest)
            .service(handlers::single)
            .service(handlers::add_comment)
            .service(handlers::static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
